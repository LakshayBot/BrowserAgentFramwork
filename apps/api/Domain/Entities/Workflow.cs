using BrowserAgent.Api.Domain.Enums;

namespace BrowserAgent.Api.Domain.Entities;

public class Workflow
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid? PluginId { get; set; }
    public WorkflowStatus Status { get; set; }
    public string? CurrentStep { get; set; }
    public string? CurrentUrl { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime? DeletedAt { get; set; }

    public User User { get; set; } = null!;
    public WorkflowPlugin? Plugin { get; set; }
    public ICollection<WorkflowStep> Steps { get; set; } = new List<WorkflowStep>();
    public BrowserSession? BrowserSession { get; set; }
    public ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();
}
