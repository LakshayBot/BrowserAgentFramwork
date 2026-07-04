using System.Text.Json;
using BrowserAgent.Api.Application.DTOs.AI;
using BrowserAgent.Api.Application.DTOs.Documents;
using BrowserAgent.Api.Application.DTOs.Profile;
using BrowserAgent.Api.Application.DTOs.Providers;
using BrowserAgent.Api.Application.Interfaces;
using BrowserAgent.Api.Browser.BrowserManager;
using BrowserAgent.Api.Browser.Interfaces;
using BrowserAgent.Api.Domain.Entities;
using BrowserAgent.Api.Domain.Enums;
using BrowserAgent.Api.Infrastructure.Data;
using Microsoft.Playwright;
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

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

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
                    var msg = $"Workflow execution failed: {ex.Message}";
                    if (msg.Length > 1900) msg = msg[..1900] + "...";
                    db.WorkflowLogs.Add(new WorkflowLog
                    {
                        Id = Guid.NewGuid(),
                        WorkflowId = workflowId,
                        Timestamp = DateTime.UtcNow,
                        Level = "Error",
                        StepName = "Fatal",
                        Message = msg,
                        Data = JsonSerializer.Serialize(new { error = ex.ToString() }, JsonOpts)
                    });
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

        void AddLog(string level, string stepName, string message, object? data = null, string? screenshotPath = null)
        {
            db.WorkflowLogs.Add(new WorkflowLog
            {
                Id = Guid.NewGuid(),
                WorkflowId = workflowId,
                Timestamp = DateTime.UtcNow,
                Level = level,
                StepName = stepName,
                Message = message,
                Data = data is not null ? JsonSerializer.Serialize(data, JsonOpts) : null,
                ScreenshotPath = screenshotPath
            });
        }

        async Task<string?> CaptureScreenshot()
        {
            if (sessionId is null) return null;
            try
            {
                var fileName = $"wf_{workflowId:N}_{DateTime.UtcNow:yyyyMMddHHmmssfff}.png";
                return await _screenshots.CaptureAsync(sessionId, fileName);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Screenshot capture failed");
                return null;
            }
        }

        try
        {
            var url = wf.CurrentUrl ?? throw new InvalidOperationException("No URL");

            // Step 1-2: Launch browser and navigate
            wf.CurrentStep = "Launching browser";
            await SaveAsync(db, wf);
            AddLog("Debug", "Browser", "Launching browser session...");

            var instance = await _browserManager.LaunchAsync(new BrowserOptions { Headless = true });
            sessionId = instance.SessionId;
            AddStepLocal( "Browser Started");
            AddLog("Info", "Browser", "Browser session launched successfully", new { sessionId });

            wf.CurrentStep = "Navigating";
            await SaveAsync(db, wf);
            AddLog("Debug", "Navigation", $"Navigating to {url}");

            var navResult = await _navigation.NavigateAsync(sessionId, url);
            if (!navResult.Success)
            {
                wf.Status = WorkflowStatus.Failed;
                wf.CurrentStep = "Navigation failed";
                AddLog("Error", "Navigation", $"Failed to navigate: {navResult.Error}");
                await SaveAsync(db, wf);
                return;
            }
            AddStepLocal( "Navigated to URL");

            var ssNav = await CaptureScreenshot();
            AddLog("Info", "Navigation", "Page loaded", new { url }, screenshotPath: ssNav);

            // Step 3: Analyze page
            wf.CurrentStep = "Analyzing page";
            await SaveAsync(db, wf);
            AddLog("Debug", "Analysis", "Extracting page DOM...");

            var page = await _extractor.ExtractPageAsync(sessionId);
            var analysis = analyzer.Analyze(page, url);
            AddStepLocal( $"Page Analysis: {analysis.Description}");

            AddLog("Info", "Analysis", "Page analysis complete", new
            {
                pageType = analysis.Type.ToString(),
                confidence = analysis.Confidence,
                description = analysis.Description,
                hasApplyButton = analysis.HasApplyButton,
                hasLoginForm = analysis.HasLoginForm,
                hasReviewIndicators = analysis.HasReviewIndicators,
                headings = page.Headings,
                buttons = page.Buttons.Select(b => new { text = b.Text, type = b.Type }),
                links = page.Links.Count,
                forms = page.Forms.Select(f => new
                {
                    id = f.FormId,
                    name = f.FormName,
                    fields = f.Fields.Select(fld => new
                    {
                        id = fld.Id,
                        label = fld.Label,
                        type = fld.FieldType,
                        required = fld.Required,
                        options = fld.AvailableOptions
                    })
                })
            });

            _logger.LogInformation("Page analysis: {Type} (conf: {Conf})", analysis.Type, analysis.Confidence);

            // Step 4: Find and click apply button
            if (analysis.HasApplyButton)
            {
                wf.CurrentStep = "Clicking Apply";
                await SaveAsync(db, wf);
                AddLog("Debug", "Apply", "Looking for apply button...");

                // Try common apply button locators with fast 5s timeouts
                var applySelectors = new[] {
                    "text=Apply Now",
                    "text=Apply now",
                    "a:has-text('Apply')",
                    "button:has-text('Apply')",
                    "button:has-text('I'm Interested')",
                    "a:has-text('I am interested')",
                    "[data-automation-id*=apply]",
                    ".apply-button",
                    ".btn-apply",
                };

                bool clicked = false;

                if (_browserManager is PlaywrightSessionManager pm)
                {
                    var session = pm.GetSession(sessionId);
                    if (session is not null)
                    {
                        var popupTcs = new TaskCompletionSource<IPage>();
                        session.Page.Popup += (_, popup) => popupTcs.TrySetResult(popup);

                        foreach (var sel in applySelectors)
                        {
                            try
                            {
                                await session.Page.ClickAsync(sel, new PageClickOptions { Timeout = 5000 });
                                clicked = true;
                                AddLog("Debug", "Apply", $"Clicked apply: {sel}");
                                break;
                            }
                            catch { }
                        }

                        if (clicked && popupTcs.Task.IsCompletedSuccessfully)
                        {
                            var popup = popupTcs.Task.Result;
                            pm.SwitchPage(sessionId, popup);
                            AddLog("Debug", "Apply", $"Popup window detected: {popup.Url}");
                        }

                        if (clicked)
                        {
                            AddLog("Info", "Apply", $"Apply button clicked, waiting for page load...");

                            try { await session.Page.WaitForLoadStateAsync(LoadState.NetworkIdle, new PageWaitForLoadStateOptions { Timeout = 15000 }); }
                            catch { try { await session.Page.WaitForLoadStateAsync(LoadState.DOMContentLoaded); } catch { } }

                            await Task.Delay(3000);

                            try { await session.Page.EvaluateAsync("window.scrollTo(0, document.body.scrollHeight)"); }
                            catch { }
                            await Task.Delay(1500);
                            try { await session.Page.EvaluateAsync("window.scrollTo(0, 0)"); }
                            catch { }
                            await Task.Delay(2000);
                            AddStepLocal( "Apply button clicked");

                            var newUrl = session.Page.Url;
                            if (!string.IsNullOrEmpty(newUrl)) url = newUrl;

                            var ssApply = await CaptureScreenshot();
                            page = await _extractor.ExtractPageAsync(sessionId);
                            analysis = analyzer.Analyze(page, url);

                            AddLog("Info", "Apply", "Page re-analyzed after apply click", new
                            {
                                currentUrl = session.Page.Url,
                                pageType = analysis.Type.ToString(),
                                description = analysis.Description
                            }, screenshotPath: ssApply);
                        }
                    }
                }

                if (!clicked)
                {
                    AddStepLocal( "No apply button found - continuing with current page");
                    AddLog("Warning", "Apply", "No apply button found on page", new
                    {
                        attemptedSelectors = applySelectors
                    });
                }
            }

            // Step 5: Check for login/registration
            if (analysis.Type == PageType.LoginRequired || analysis.HasLoginForm)
            {
                wf.Status = WorkflowStatus.Paused;
                wf.CurrentStep = $"AwaitingHumanVerification:Login";
                AddStepLocal( "Login required - manual intervention needed");
                var ssLogin = await CaptureScreenshot();
                AddLog("Warning", "Login", "Login form detected - workflow paused", new
                {
                    message = "Manual login needed. Resume the workflow after logging in."
                }, screenshotPath: ssLogin);
                await SaveAsync(db, wf);
                return;
            }

            if (analysis.Type == PageType.HumanVerification)
            {
                wf.Status = WorkflowStatus.Paused;
                wf.CurrentStep = $"AwaitingHumanVerification:{analysis.Type}";
                AddStepLocal( $"Human verification detected: {analysis.Description}");
                var ssCaptcha = await CaptureScreenshot();
                AddLog("Warning", "Verification", "Human verification detected - workflow paused", new
                {
                    type = analysis.Type.ToString(),
                    description = analysis.Description
                }, screenshotPath: ssCaptcha);
                await SaveAsync(db, wf);
                return;
            }

            if (analysis.Type == PageType.ReviewPage)
            {
                wf.CurrentStep = "Review ready";
                AddStepLocal( "Review page reached - paused before submit");
                var ssReview = await CaptureScreenshot();
                AddLog("Info", "Review", "Review page reached", screenshotPath: ssReview);
                await SaveAsync(db, wf);
                return;
            }

            // Step 6-7: Extract forms and map fields
            wf.CurrentStep = "Extracting forms";
            await SaveAsync(db, wf);
            AddLog("Debug", "Forms", "Extracting form fields...");

            var forms = page.Forms;

            var profile = await userService.GetProfileAsync(userId);
            var profileJson = JsonSerializer.Serialize(profile);
            var documents = await docService.GetAllAsync(userId);
            var resume = documents.FirstOrDefault(d => d.DocumentType == "Resume");
            var providers = await providerService.GetAllAsync(userId);
            var defaultProvider = providers.FirstOrDefault(p => p.IsDefault) ?? providers.FirstOrDefault();

            if (forms.Count == 0 && analysis.Type == PageType.ApplicationForm && defaultProvider is not null)
            {
                wf.CurrentStep = "AI analyzing page HTML";
                await SaveAsync(db, wf);
                AddLog("Debug", "AI", "No structured forms - capturing page HTML for AI analysis...");

                string? pageHtml = null;
                if (_browserManager is PlaywrightSessionManager pmHtml)
                {
                    var sessionHtml = pmHtml.GetSession(sessionId);
                    if (sessionHtml is not null)
                    {
                        try { pageHtml = await sessionHtml.Page.ContentAsync(); }
                        catch (Exception ex) { _logger.LogWarning(ex, "Failed to capture HTML"); }
                    }
                }

                if (!string.IsNullOrEmpty(pageHtml))
                {
                    AddLog("Debug", "AI", $"Page HTML captured ({pageHtml.Length} chars) - sending to AI");

                    await TryAiFieldMappingAsync(db, wf, url, pageHtml, null, profileJson, resume, defaultProvider, sessionId);
                    await db.SaveChangesAsync();
                    stepCounter = await db.WorkflowSteps
                        .Where(x => x.WorkflowId == workflowId)
                        .MaxAsync(x => (int?)x.StepNumber) ?? stepCounter;

                    if (wf.Status == WorkflowStatus.Completed)
                        return;
                }
                else
                {
                    AddStepLocal( "No forms found on page");
                    wf.Status = WorkflowStatus.Completed;
                    wf.CompletedAt = DateTime.UtcNow;
                    wf.CurrentStep = "No forms on page";
                    AddLog("Warning", "Forms", "HTML capture failed, no forms to fill");
                    await SaveAsync(db, wf);
                    return;
                }
            }
            else if (forms.Count == 0)
            {
                AddStepLocal( "No forms found on page");
                wf.Status = WorkflowStatus.Completed;
                wf.CompletedAt = DateTime.UtcNow;
                wf.CurrentStep = "No forms on page";
                var ssNoForm = await CaptureScreenshot();

                if (analysis.Type == PageType.ApplicationForm)
                    AddLog("Warning", "Forms", "Application form detected but no fields extracted. Configure an AI provider for HTML-based analysis.", screenshotPath: ssNoForm);
                else
                    AddLog("Warning", "Forms", "No forms detected on page", screenshotPath: ssNoForm);

                await SaveAsync(db, wf);
                return;
            }

            var form = forms.FirstOrDefault();
            if (form is not null)
            {
                AddStepLocal( $"Form extracted: {form.Fields.Count} fields");

                AddLog("Info", "Forms", "Form extracted", new
                {
                    formId = form.FormId,
                    formName = form.FormName,
                    fieldCount = form.Fields.Count,
                    fields = form.Fields.Select(f => new
                    {
                        id = f.Id, label = f.Label, placeholder = f.Placeholder,
                        type = f.FieldType, required = f.Required, disabled = f.Disabled,
                        currentValue = f.CurrentValue, options = f.AvailableOptions
                    })
                });

                if (defaultProvider is not null && form.Fields.Count > 0)
                {
                    wf.CurrentStep = "AI field mapping";
                    await SaveAsync(db, wf);
                    AddLog("Debug", "AI", "Calling AI for structured field mapping...", new
                    {
                        provider = defaultProvider.ProviderType.ToString(),
                        model = defaultProvider.ModelName,
                        fieldCount = form.Fields.Count
                    });

                    await TryAiFieldMappingAsync(db, wf, url, null, form, profileJson, resume, defaultProvider, sessionId);
                    await db.SaveChangesAsync();
                    stepCounter = await db.WorkflowSteps
                        .Where(x => x.WorkflowId == workflowId)
                        .MaxAsync(x => (int?)x.StepNumber) ?? stepCounter;
                }
                else
                {
                    AddLog("Warning", "AI", "AI field mapping skipped - no AI provider configured");
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
                var ssFinal = await CaptureScreenshot();
                AddLog("Info", "Review", "Review page confirmed", new
                {
                    nextActions = nextButtons.Select(b => b.Text)
                }, screenshotPath: ssFinal);
                await SaveAsync(db, wf);
                return;
            }

            wf.CurrentStep = "Workflow execution complete";
            wf.Status = WorkflowStatus.Completed;
            wf.CompletedAt = DateTime.UtcNow;
            AddStepLocal( "Execution completed");
            var ssDone = await CaptureScreenshot();
            AddLog("Info", "Complete", "Workflow execution finished", screenshotPath: ssDone);
            await SaveAsync(db, wf);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Workflow step failed: {WorkflowId}", workflowId);
            wf.Status = WorkflowStatus.Failed;
            wf.CompletedAt = DateTime.UtcNow;
            wf.CurrentStep = "Failed";
            AddStepLocal( $"Error: {ex.Message}");
            var ssError = await CaptureScreenshot();
            AddLog("Error", "Failed", $"Workflow failed: {ex.Message}", new
            {
                error = ex.ToString()
            }, screenshotPath: ssError);
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

    private async Task TryAiFieldMappingAsync(
        AppDbContext db, Workflow wf, string url,
        string? pageHtml, ExtractedForm? form,
        string profileJson, DocumentDto? resume,
        ProviderDto defaultProvider, string? sessionId)
    {
        var stepNum = await db.WorkflowSteps
            .Where(x => x.WorkflowId == wf.Id)
            .MaxAsync(x => (int?)x.StepNumber) ?? 0;

        void AddStep(string name)
        {
            stepNum++;
            db.WorkflowSteps.Add(new WorkflowStep
            {
                Id = Guid.NewGuid(), WorkflowId = wf.Id, StepNumber = stepNum,
                StepName = name, Status = StepStatus.Completed,
                StartedAt = DateTime.UtcNow, CompletedAt = DateTime.UtcNow
            });
        }

        void AddLogEntry(string level, string stepName, string message, object? data = null, string? screenshotPath = null)
        {
            db.WorkflowLogs.Add(new WorkflowLog
            {
                Id = Guid.NewGuid(), WorkflowId = wf.Id, Timestamp = DateTime.UtcNow,
                Level = level, StepName = stepName, Message = message,
                Data = data is not null ? JsonSerializer.Serialize(data, JsonOpts) : null,
                ScreenshotPath = screenshotPath
            });
        }

        try
        {
            var request = new FieldMapRequest
            {
                PageHtml = pageHtml ?? "",
                PageSchema = new Dictionary<string, object> { ["url"] = url },
                FormSchema = form is not null
                    ? new Dictionary<string, object> { ["fields"] = form.Fields.Select(f => new {
                        id = f.Id, label = f.Label, type = f.FieldType, required = f.Required
                    }).ToList() }
                    : new Dictionary<string, object>(),
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
            };

            var fieldMap = await _aiClient.MapFieldsAsync(request);

            AddStep( $"AI mapped {fieldMap.Mappings.Count} fields (conf: {fieldMap.ConfidenceOverall:F2})");

            AddLogEntry("Info", "AI", "AI field mapping complete", new
            {
                confidenceOverall = fieldMap.ConfidenceOverall,
                mappings = fieldMap.Mappings.Select(m => new
                {
                    fieldId = m.FieldId, selector = m.Selector,
                    value = m.Value, confidence = m.Confidence, source = m.Source
                }),
                usedFields = fieldMap.Mappings.Count(m => m.Confidence > 0.3)
            });

            wf.CurrentStep = "Filling form";
            await db.SaveChangesAsync();

            AddLogEntry("Debug", "AI", "Filling form fields...");

            int filledCount = 0;
            foreach (var mapping in fieldMap.Mappings.Where(m => m.Confidence > 0.3))
            {
                if (string.IsNullOrEmpty(mapping.Value)) continue;

                var selector = !string.IsNullOrEmpty(mapping.Selector)
                    ? mapping.Selector
                    : form?.Fields.FirstOrDefault(f => f.Id == mapping.FieldId) is { } matched
                        ? $"#{matched.Id}"
                        : null;

                if (selector is null) continue;

                try
                {
                    var field = form?.Fields.FirstOrDefault(f => f.Id == mapping.FieldId);
                    if (field?.FieldType is "select" or "dropdown")
                        await _interaction.SelectAsync(sessionId, selector, mapping.Value);
                    else
                        await _interaction.TypeAsync(sessionId, selector, mapping.Value);
                    filledCount++;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to fill field {FieldId} via selector {Selector}", mapping.FieldId, selector);
                }
            }

            AddStep( $"Form filled: {filledCount} fields");
            AddLogEntry("Info", "AI", $"Form filled with {filledCount} field values");

            string? ssFilled = null;
            if (sessionId is not null)
            {
                try
                {
                    var fileName = $"wf_{wf.Id:N}_{DateTime.UtcNow:yyyyMMddHHmmssfff}.png";
                    ssFilled = await _screenshots.CaptureAsync(sessionId, fileName);
                }
                catch { }
            }
            if (ssFilled is not null)
            {
                AddLogEntry("Info", "Screenshot", "Form after filling", screenshotPath: ssFilled);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "AI field mapping failed");

            AddStep( "AI mapping failed - continuing");
            AddLogEntry("Error", "AI", "AI field mapping failed", new
            {
                error = ex.Message,
                fallback = "Continuing without AI field mapping"
            });
        }
    }

    private static async Task SaveAsync(AppDbContext db, Workflow wf)
    {
        await db.SaveChangesAsync();
    }
}
