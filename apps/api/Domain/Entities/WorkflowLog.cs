namespace BrowserAgent.Api.Domain.Entities;

public class WorkflowLog
{
    public Guid Id { get; set; }
    public Guid WorkflowId { get; set; }
    public DateTime Timestamp { get; set; }
    public string Level { get; set; } = "Info";
    public string? StepName { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? Data { get; set; }
    public string? ScreenshotPath { get; set; }

    public Workflow Workflow { get; set; } = null!;
}
