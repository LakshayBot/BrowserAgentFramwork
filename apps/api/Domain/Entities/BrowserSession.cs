namespace BrowserAgent.Api.Domain.Entities;

public class BrowserSession
{
    public Guid Id { get; set; }
    public Guid WorkflowId { get; set; }
    public string BrowserType { get; set; } = "Chromium";
    public string? SessionIdentifier { get; set; }
    public string? CurrentUrl { get; set; }
    public string? CurrentTitle { get; set; }
    public string? LastScreenshot { get; set; }
    public DateTime CreatedAt { get; set; }

    public Workflow Workflow { get; set; } = null!;
}
