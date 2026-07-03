namespace BrowserAgent.Api.Application.Interfaces;

public interface IWorkflowEventBus
{
    Task PublishAsync(string eventType, Guid workflowId, Guid userId);
}

public class WorkflowEventBus : IWorkflowEventBus
{
    private readonly ILogger<WorkflowEventBus> _logger;

    public WorkflowEventBus(ILogger<WorkflowEventBus> logger)
    {
        _logger = logger;
    }

    public Task PublishAsync(string eventType, Guid workflowId, Guid userId)
    {
        _logger.LogInformation("Workflow event: {EventType} | Workflow: {WorkflowId} | User: {UserId}",
            eventType, workflowId, userId);
        return Task.CompletedTask;
    }
}
