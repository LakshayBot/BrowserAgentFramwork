namespace BrowserAgent.Api.Plugins;

public interface IWorkflowPlugin
{
    string Name { get; }
    string DisplayName { get; }
    string Description { get; }
    string Version { get; }
    bool CanHandle(string url);
    IReadOnlyList<PluginStep> Steps { get; }
}

public class PluginStep
{
    public int Number { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
}

public class PluginInfo
{
    public string Name { get; init; } = string.Empty;
    public string DisplayName { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string Version { get; init; } = string.Empty;
    public List<PluginStep> Steps { get; init; } = new();
}
