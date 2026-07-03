using System.Collections.Concurrent;

namespace BrowserAgent.Api.Plugins;

public class PluginLoader
{
    private readonly ConcurrentDictionary<string, IWorkflowPlugin> _plugins = new();

    public PluginLoader(IEnumerable<IWorkflowPlugin> plugins)
    {
        foreach (var plugin in plugins)
        {
            _plugins[plugin.Name] = plugin;
        }
    }

    public IWorkflowPlugin? GetPlugin(string name)
    {
        _plugins.TryGetValue(name, out var plugin);
        return plugin;
    }

    public IWorkflowPlugin? ResolveForUrl(string url)
    {
        return _plugins.Values.FirstOrDefault(p => p.CanHandle(url));
    }

    public IReadOnlyList<IWorkflowPlugin> GetAll() => _plugins.Values.ToList();

    public List<PluginInfo> GetPluginInfos()
    {
        return _plugins.Values.Select(p => new PluginInfo
        {
            Name = p.Name,
            DisplayName = p.DisplayName,
            Description = p.Description,
            Version = p.Version,
            Steps = p.Steps.Select(s => new PluginStep
            {
                Number = s.Number,
                Name = s.Name,
                Description = s.Description
            }).ToList()
        }).ToList();
    }
}
