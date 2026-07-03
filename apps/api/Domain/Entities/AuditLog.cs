namespace BrowserAgent.Api.Domain.Entities;

public class AuditLog
{
    public Guid Id { get; set; }
    public Guid? WorkflowId { get; set; }
    public Guid? UserId { get; set; }
    public string EventType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }

    public Workflow? Workflow { get; set; }
    public User? User { get; set; }
}
