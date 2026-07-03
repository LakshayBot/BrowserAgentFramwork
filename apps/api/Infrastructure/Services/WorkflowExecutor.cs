using System.Text.Json;
using BrowserAgent.Api.Application.DTOs.AI;
using BrowserAgent.Api.Application.DTOs.Profile;
using BrowserAgent.Api.Application.Interfaces;
using BrowserAgent.Api.Browser.Interfaces;
using BrowserAgent.Api.Domain.Entities;
using BrowserAgent.Api.Domain.Enums;
using BrowserAgent.Api.Infrastructure.Data;
using BrowserAgent.Api.Plugins;
using BrowserAgent.Api.Plugins.JobApplication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BrowserAgent.Api.Infrastructure.Services;

public class WorkflowExecutor
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IBrowserManager _browserManager;
    private readonly INavigationService _navigation;
    private readonly IFormExtractor _extractor;
    private readonly IInteractionService _interaction;
    private readonly IScreenshotService _screenshots;
    private readonly IAiClient _aiClient;
    private readonly ILogger<WorkflowExecutor> _logger;

    public WorkflowExecutor(
        IServiceScopeFactory scopeFactory,
        IBrowserManager browserManager,
        INavigationService navigation,
        IFormExtractor extractor,
        IInteractionService interaction,
        IScreenshotService screenshots,
        IAiClient aiClient,
        ILogger<WorkflowExecutor> logger)
    {
        _scopeFactory = scopeFactory;
        _browserManager = browserManager;
        _navigation = navigation;
        _extractor = extractor;
        _interaction = interaction;
        _screenshots = screenshots;
        _aiClient = aiClient;
        _logger = logger;
    }

    public void ExecuteBackground(Guid workflowId, Guid userId)
    {
        _ = Task.Run(async () =>
        {
            try
            {
                await ExecuteAsync(workflowId, userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Workflow execution failed: {WorkflowId}", workflowId);
                using var scope = _scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var wf = await db.Workflows.FindAsync(workflowId);
                if (wf is not null)
                {
                    wf.Status = WorkflowStatus.Failed;
                    wf.CompletedAt = DateTime.UtcNow;
                    wf.CurrentStep = "Failed";
                    await db.SaveChangesAsync();
                }
            }
        });
    }

    private async Task ExecuteAsync(Guid workflowId, Guid userId)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var userService = scope.ServiceProvider.GetRequiredService<IUserService>();
        var docService = scope.ServiceProvider.GetRequiredService<IDocumentService>();
        var providerService = scope.ServiceProvider.GetRequiredService<IProviderService>();

        var wf = await db.Workflows.FindAsync(workflowId);
        if (wf is null || wf.UserId != userId) return;

        var analyzer = new PageAnalyzer();
        string? sessionId = null;
        var stepCounter = await db.WorkflowSteps
            .Where(x => x.WorkflowId == workflowId)
            .MaxAsync(x => (int?)x.StepNumber) ?? 0;

        void AddStepLocal(string name)
        {
            stepCounter++;
            db.WorkflowSteps.Add(new WorkflowStep
            {
                Id = Guid.NewGuid(),
                WorkflowId = workflowId,
                StepNumber = stepCounter,
                StepName = name,
                Status = StepStatus.Completed,
                StartedAt = DateTime.UtcNow,
                CompletedAt = DateTime.UtcNow
            });
        }

        try
        {
            var url = wf.CurrentUrl ?? throw new InvalidOperationException("No URL");

            // Step 1-2: Launch browser and navigate
            wf.CurrentStep = "Launching browser";
            await SaveAsync(db, wf);

            var instance = await _browserManager.LaunchAsync(new BrowserOptions { Headless = true });
            sessionId = instance.SessionId;
            AddStepLocal( "Browser Started");

            wf.CurrentStep = "Navigating";
            await SaveAsync(db, wf);

            var navResult = await _navigation.NavigateAsync(sessionId, url);
            if (!navResult.Success)
            {
                wf.Status = WorkflowStatus.Failed;
                wf.CurrentStep = "Navigation failed";
                await SaveAsync(db, wf);
                return;
            }
            AddStepLocal( "Navigated to URL");

            // Step 3: Analyze page
            wf.CurrentStep = "Analyzing page";
            await SaveAsync(db, wf);

            var page = await _extractor.ExtractPageAsync(sessionId);
            var analysis = analyzer.Analyze(page, url);
            AddStepLocal( $"Page Analysis: {analysis.Description}");

            _logger.LogInformation("Page analysis: {Type} (conf: {Conf})", analysis.Type, analysis.Confidence);

            // Step 4: Find and click apply button
            if (analysis.HasApplyButton)
            {
                wf.CurrentStep = "Clicking Apply";
                await SaveAsync(db, wf);

                // Try common apply button locators
                var applySelectors = new[] {
                    "button:has-text('Apply')",
                    "a:has-text('Apply Now')",
                    "button:has-text('I'm Interested')",
                    "a:has-text('Apply')",
                };

                bool clicked = false;
                foreach (var sel in applySelectors)
                {
                    var result = await _interaction.ClickAsync(sessionId, sel);
                    if (result.Success) { clicked = true; break; }
                }

                if (clicked)
                {
                    await Task.Delay(2000);
                    await _navigation.NavigateAsync(sessionId, url); // wait for page load
                    AddStepLocal( "Apply button clicked");
                    page = await _extractor.ExtractPageAsync(sessionId);
                    analysis = analyzer.Analyze(page, url);
                }
                else
                {
                    AddStepLocal( "No apply button found - continuing with current page");
                }
            }

            // Step 5: Check for login/registration
            if (analysis.Type == PageType.LoginRequired || analysis.HasLoginForm)
            {
                wf.Status = WorkflowStatus.Paused;
                wf.CurrentStep = $"AwaitingHumanVerification:Login";
                AddStepLocal( "Login required - manual intervention needed");
                await SaveAsync(db, wf);
                return;
            }

            if (analysis.Type == PageType.HumanVerification)
            {
                wf.Status = WorkflowStatus.Paused;
                wf.CurrentStep = $"AwaitingHumanVerification:{analysis.Type}";
                AddStepLocal( $"Human verification detected: {analysis.Description}");
                await SaveAsync(db, wf);
                return;
            }

            if (analysis.Type == PageType.ReviewPage)
            {
                wf.CurrentStep = "Review ready";
                AddStepLocal( "Review page reached - paused before submit");
                await SaveAsync(db, wf);
                return;
            }

            // Step 6-7: Extract forms and map fields
            wf.CurrentStep = "Extracting forms";
            await SaveAsync(db, wf);

            var forms = page.Forms;
            if (forms.Count == 0)
            {
                AddStepLocal( "No forms found on page");
                await SaveAsync(db, wf);
                return;
            }

            var form = forms[0];
            AddStepLocal( $"Form extracted: {form.Fields.Count} fields");

            // Get user profile
            var profile = await userService.GetProfileAsync(userId);
            var profileJson = JsonSerializer.Serialize(profile);

            // Get documents
            var documents = await docService.GetAllAsync(userId);
            var resume = documents.FirstOrDefault(d => d.DocumentType == "Resume");

            // Get default provider
            var providers = await providerService.GetAllAsync(userId);
            var defaultProvider = providers.FirstOrDefault(p => p.IsDefault) ?? providers.FirstOrDefault();

            if (defaultProvider is not null && form.Fields.Count > 0)
            {
                wf.CurrentStep = "AI field mapping";
                await SaveAsync(db, wf);

                try
                {
                    var fieldMap = await _aiClient.MapFieldsAsync(new FieldMapRequest
                    {
                        PageSchema = new Dictionary<string, object> { ["url"] = url },
                        FormSchema = new Dictionary<string, object> { ["fields"] = form.Fields.Select(f => new {
                            id = f.Id, label = f.Label, type = f.FieldType, required = f.Required
                        }).ToList() },
                        Profile = JsonSerializer.Deserialize<Dictionary<string, object>>(profileJson) ?? new(),
                        Resume = resume is not null ? new Dictionary<string, object> { ["name"] = resume.DisplayName } : new(),
                        Provider = new ProviderConfigDto
                        {
                            ProviderType = defaultProvider.ProviderType,
                            ModelName = defaultProvider.ModelName,
                            BaseUrl = defaultProvider.BaseUrl ?? "",
                            Temperature = defaultProvider.Temperature,
                            MaxTokens = defaultProvider.MaxTokens
                        }
                    });

                    AddStepLocal( $"AI mapped {fieldMap.Mappings.Count} fields (conf: {fieldMap.ConfidenceOverall:F2})");

                    // Step 8: Fill form fields
                    wf.CurrentStep = "Filling form";
                    await SaveAsync(db, wf);

                    foreach (var mapping in fieldMap.Mappings.Where(m => m.Confidence > 0.3))
                    {
                        if (string.IsNullOrEmpty(mapping.Value)) continue;
                        var field = form.Fields.FirstOrDefault(f => f.Id == mapping.FieldId);
                        if (field is null) continue;

                        try
                        {
                            if (field.FieldType is "select" or "dropdown")
                                await _interaction.SelectAsync(sessionId, $"#{field.Id}", mapping.Value);
                            else
                                await _interaction.TypeAsync(sessionId, $"#{field.Id}", mapping.Value);
                        }
                        catch { }
                    }

                    AddStepLocal( $"Form filled: {fieldMap.Mappings.Count(m => m.Confidence > 0.3)} fields");
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "AI field mapping failed, continuing without AI");
                    AddStepLocal( "AI mapping failed - continuing");
                }
            }

            // Check for next/submit buttons
            var nextButtons = page.Buttons.Where(b =>
                (b.Text ?? "").Contains("Next", StringComparison.OrdinalIgnoreCase) ||
                (b.Text ?? "").Contains("Continue", StringComparison.OrdinalIgnoreCase) ||
                (b.Text ?? "").Contains("Submit", StringComparison.OrdinalIgnoreCase)
            ).ToList();

            if (nextButtons.Any(b => (b.Text ?? "").Contains("Submit", StringComparison.OrdinalIgnoreCase) && analysis.HasReviewIndicators))
            {
                wf.CurrentStep = "Review ready - do not submit";
                AddStepLocal( "Review page detected");
                await SaveAsync(db, wf);
                return;
            }

            wf.CurrentStep = "Workflow execution complete";
            wf.Status = WorkflowStatus.Completed;
            wf.CompletedAt = DateTime.UtcNow;
            AddStepLocal( "Execution completed");
            await SaveAsync(db, wf);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Workflow step failed: {WorkflowId}", workflowId);
            wf.Status = WorkflowStatus.Failed;
            wf.CompletedAt = DateTime.UtcNow;
            wf.CurrentStep = "Failed";
            AddStepLocal( $"Error: {ex.Message}");
            await SaveAsync(db, wf);
        }
        finally
        {
            if (sessionId is not null)
            {
                try { await _browserManager.CloseAsync(sessionId); }
                catch { }
            }
        }
    }

    private static async Task SaveAsync(AppDbContext db, Workflow wf)
    {
        await db.SaveChangesAsync();
    }
}
