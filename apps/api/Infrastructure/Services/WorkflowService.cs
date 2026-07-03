using BrowserAgent.Api.Application.DTOs.Workflows;
using BrowserAgent.Api.Application.Interfaces;
using BrowserAgent.Api.Domain.Entities;
using BrowserAgent.Api.Domain.Enums;
using BrowserAgent.Api.Domain.Exceptions;
using BrowserAgent.Api.Infrastructure.Data;
using BrowserAgent.Api.Plugins;
using Microsoft.EntityFrameworkCore;

namespace BrowserAgent.Api.Infrastructure.Services;

public class WorkflowService : IWorkflowService
{
    private readonly AppDbContext _context;
    private readonly IWorkflowEventBus _eventBus;
    private readonly PluginLoader _pluginLoader;
    private readonly ILogger<WorkflowService> _logger;

    public WorkflowService(
        AppDbContext context,
        IWorkflowEventBus eventBus,
        PluginLoader pluginLoader,
        ILogger<WorkflowService> logger)
    {
        _context = context;
        _eventBus = eventBus;
        _pluginLoader = pluginLoader;
        _logger = logger;
    }

    public async Task<WorkflowDto> CreateAsync(Guid userId, CreateWorkflowRequest request, CancellationToken ct = default)
    {
        var pluginId = await ResolvePluginId(request.PluginName, ct);

        var workflow = new Workflow
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            PluginId = pluginId,
            Status = WorkflowStatus.Created,
            CurrentStep = "Initializing",
            CurrentUrl = request.Url,
            CreatedAt = DateTime.UtcNow
        };

        workflow.Steps.Add(new WorkflowStep
        {
            Id = Guid.NewGuid(),
            WorkflowId = workflow.Id,
            StepNumber = 1,
            StepName = "Workflow Created",
            Status = StepStatus.Completed,
            StartedAt = DateTime.UtcNow,
            CompletedAt = DateTime.UtcNow
        });

        _context.Workflows.Add(workflow);
        await _context.SaveChangesAsync(ct);

        _logger.LogInformation("Workflow created: {Id} for user {UserId} URL: {Url}",
            workflow.Id, userId, request.Url);

        await _eventBus.PublishAsync("WorkflowCreated", workflow.Id, userId);

        return MapToDto(workflow);
    }

    public async Task<WorkflowDto> StartAsync(Guid id, Guid userId, CancellationToken ct = default)
    {
        var workflow = await GetWorkflowAsync(id, userId, ct);

        if (workflow.Status != WorkflowStatus.Created)
            throw new DomainException("INVALID_STATE", $"Cannot start a workflow in '{workflow.Status}' state. Must be Created.");

        var plugin = _pluginLoader.ResolveForUrl(workflow.CurrentUrl ?? "");
        if (plugin is null)
        {
            workflow.Status = WorkflowStatus.Failed;
            workflow.CompletedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(ct);
            throw new DomainException("NO_PLUGIN", $"No plugin found to handle URL: {workflow.CurrentUrl}");
        }

        workflow.Status = WorkflowStatus.Running;
        workflow.StartedAt = DateTime.UtcNow;
        workflow.CurrentStep = "Starting";
        await AddStepAsync(workflow, "Workflow Started");
        await _context.SaveChangesAsync(ct);

        _logger.LogInformation("Workflow started: {Id} with plugin: {Plugin}", id, plugin.Name);
        await _eventBus.PublishAsync("WorkflowStarted", workflow.Id, userId);

        return await GetByIdAsync(id, userId, ct);
    }

    public async Task<WorkflowDto> GetByIdAsync(Guid id, Guid userId, CancellationToken ct = default)
    {
        var workflow = await _context.Workflows
            .AsNoTracking()
            .Include(x => x.Steps.OrderBy(s => s.StepNumber))
            .FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId, ct);

        if (workflow is null)
            throw new NotFoundException("Workflow", id);

        return MapToDto(workflow);
    }

    public async Task<List<WorkflowSummaryDto>> GetAllAsync(Guid userId, int page = 1, int pageSize = 20, CancellationToken ct = default)
    {
        return await _context.Workflows
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new WorkflowSummaryDto
            {
                Id = x.Id,
                Status = x.Status.ToString(),
                CurrentStep = x.CurrentStep,
                CurrentUrl = x.CurrentUrl,
                CreatedAt = x.CreatedAt
            })
            .ToListAsync(ct);
    }

    public async Task<WorkflowDto> PauseAsync(Guid id, Guid userId, CancellationToken ct = default)
    {
        var workflow = await GetWorkflowAsync(id, userId, ct);

        if (workflow.Status != WorkflowStatus.Running)
            throw new DomainException("INVALID_STATE", $"Cannot pause a workflow in '{workflow.Status}' state. Must be Running.");

        workflow.Status = WorkflowStatus.Paused;
        workflow.CurrentStep = "Paused";
        await AddStepAsync(workflow, "Paused");
        await _context.SaveChangesAsync(ct);

        _logger.LogInformation("Workflow paused: {Id}", workflow.Id);
        await _eventBus.PublishAsync("WorkflowPaused", workflow.Id, userId);

        return await GetByIdAsync(id, userId, ct);
    }

    public async Task<WorkflowDto> PauseForVerificationAsync(Guid id, Guid userId, string verificationType, string? screenshotPath = null, CancellationToken ct = default)
    {
        var workflow = await GetWorkflowAsync(id, userId, ct);

        if (workflow.Status != WorkflowStatus.Running)
            throw new DomainException("INVALID_STATE", $"Cannot pause a workflow in '{workflow.Status}' state. Must be Running.");

        workflow.Status = WorkflowStatus.Paused;
        workflow.CurrentStep = $"AwaitingHumanVerification:{verificationType}";
        await AddStepAsync(workflow, $"Human Verification Required: {verificationType}");
        await _context.SaveChangesAsync(ct);

        _logger.LogInformation("Workflow paused for human verification: {Id} type={VerificationType} screenshot={Screenshot}",
            workflow.Id, verificationType, screenshotPath);

        await _eventBus.PublishAsync("HumanVerificationRequired", workflow.Id, userId);

        return await GetByIdAsync(id, userId, ct);
    }

    public async Task<WorkflowDto> ResumeAsync(Guid id, Guid userId, CancellationToken ct = default)
    {
        var workflow = await GetWorkflowAsync(id, userId, ct);

        if (workflow.Status != WorkflowStatus.Paused)
            throw new DomainException("INVALID_STATE", $"Cannot resume a workflow in '{workflow.Status}' state. Must be Paused.");

        var wasVerificationPause = workflow.CurrentStep?.StartsWith("AwaitingHumanVerification") == true;

        workflow.Status = WorkflowStatus.Running;
        workflow.CurrentStep = wasVerificationPause ? "ResumedAfterVerification" : "Resumed";
        await AddStepAsync(workflow, wasVerificationPause ? "Human Verification Completed" : "Resumed");
        await _context.SaveChangesAsync(ct);

        _logger.LogInformation("Workflow resumed: {Id} (was verification pause: {WasVerification})",
            workflow.Id, wasVerificationPause);

        await _eventBus.PublishAsync(wasVerificationPause ? "WorkflowResumedAfterVerification" : "WorkflowResumed", workflow.Id, userId);

        return await GetByIdAsync(id, userId, ct);
    }

    public async Task<WorkflowDto> CancelAsync(Guid id, Guid userId, CancellationToken ct = default)
    {
        var workflow = await GetWorkflowAsync(id, userId, ct);

        if (workflow.Status is WorkflowStatus.Completed or WorkflowStatus.Cancelled)
            throw new DomainException("INVALID_STATE", $"Cannot cancel a workflow in '{workflow.Status}' state.");

        workflow.Status = WorkflowStatus.Cancelled;
        workflow.CompletedAt = DateTime.UtcNow;
        await AddStepAsync(workflow, "Cancelled");
        await _context.SaveChangesAsync(ct);

        _logger.LogInformation("Workflow cancelled: {Id}", workflow.Id);
        await _eventBus.PublishAsync("WorkflowCancelled", workflow.Id, userId);

        return await GetByIdAsync(id, userId, ct);
    }

    public async Task DeleteAsync(Guid id, Guid userId, CancellationToken ct = default)
    {
        var workflow = await _context.Workflows
            .FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId, ct);

        if (workflow is null)
            throw new NotFoundException("Workflow", id);

        _context.Workflows.Remove(workflow);
        await _context.SaveChangesAsync(ct);

        _logger.LogInformation("Workflow deleted: {Id}", workflow.Id);
    }

    private async Task<Workflow> GetWorkflowAsync(Guid id, Guid userId, CancellationToken ct)
    {
        var workflow = await _context.Workflows
            .FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId, ct);

        if (workflow is null)
            throw new NotFoundException("Workflow", id);

        return workflow;
    }

    private async Task AddStepAsync(Workflow workflow, string stepName)
    {
        var maxStep = await _context.WorkflowSteps
            .Where(x => x.WorkflowId == workflow.Id)
            .MaxAsync(x => (int?)x.StepNumber) ?? 0;

        _context.WorkflowSteps.Add(new WorkflowStep
        {
            Id = Guid.NewGuid(),
            WorkflowId = workflow.Id,
            StepNumber = maxStep + 1,
            StepName = stepName,
            Status = StepStatus.Completed,
            StartedAt = DateTime.UtcNow,
            CompletedAt = DateTime.UtcNow
        });
    }

    private async Task<Guid?> ResolvePluginId(string? pluginName, CancellationToken ct)
    {
        if (string.IsNullOrEmpty(pluginName))
            return null;

        var plugin = await _context.WorkflowPlugins
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.PluginName == pluginName && x.Enabled, ct);

        return plugin?.Id;
    }

    private static WorkflowDto MapToDto(Workflow workflow)
    {
        return new WorkflowDto
        {
            Id = workflow.Id,
            UserId = workflow.UserId,
            Status = workflow.Status.ToString(),
            CurrentStep = workflow.CurrentStep,
            CurrentUrl = workflow.CurrentUrl,
            CreatedAt = workflow.CreatedAt,
            StartedAt = workflow.StartedAt,
            CompletedAt = workflow.CompletedAt,
            Steps = workflow.Steps?.Select(s => new WorkflowStepDto
            {
                StepNumber = s.StepNumber,
                StepName = s.StepName,
                Status = s.Status.ToString(),
                StartedAt = s.StartedAt,
                CompletedAt = s.CompletedAt,
                ErrorMessage = s.ErrorMessage
            }).ToList() ?? new()
        };
    }
}
