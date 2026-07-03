namespace BrowserAgent.Api.Domain.Entities;

public class WorkflowPlugin
{
    public Guid Id { get; set; }
    public string PluginName { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public bool Enabled { get; set; } = true;
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
}
