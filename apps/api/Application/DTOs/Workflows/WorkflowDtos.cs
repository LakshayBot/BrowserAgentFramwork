namespace BrowserAgent.Api.Application.DTOs.Workflows;

public class CreateWorkflowRequest
{
    public string Url { get; set; } = string.Empty;
    public string? PluginName { get; set; }
}

public class WorkflowDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? CurrentStep { get; set; }
    public string? CurrentUrl { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public List<WorkflowStepDto> Steps { get; set; } = new();
}

public class WorkflowStepDto
{
    public int StepNumber { get; set; }
    public string StepName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? ErrorMessage { get; set; }
}

public class WorkflowSummaryDto
{
    public Guid Id { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? CurrentStep { get; set; }
    public string? CurrentUrl { get; set; }
    public DateTime CreatedAt { get; set; }
}
