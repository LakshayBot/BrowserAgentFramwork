using BrowserAgent.Api.Plugins;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BrowserAgent.Api.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/plugins")]
public class PluginsController : ControllerBase
{
    private readonly PluginLoader _pluginLoader;

    public PluginsController(PluginLoader pluginLoader)
    {
        _pluginLoader = pluginLoader;
    }

    /// <summary>
    /// List all installed workflow plugins.
    /// </summary>
    [HttpGet]
    public IActionResult GetAll()
    {
        var result = _pluginLoader.GetPluginInfos();
        return Ok(ApiResponse<List<PluginInfo>>.Ok(result));
    }

    /// <summary>
    /// Get details for a specific plugin.
    /// </summary>
    [HttpGet("{name}")]
    public IActionResult GetByName(string name)
    {
        var plugin = _pluginLoader.GetPlugin(name);
        if (plugin is null)
            return NotFound(ApiResponse<object>.Fail("NOT_FOUND", $"Plugin '{name}' not found"));

        var info = new PluginInfo
        {
            Name = plugin.Name,
            DisplayName = plugin.DisplayName,
            Description = plugin.Description,
            Version = plugin.Version,
            Steps = plugin.Steps.Select(s => new PluginStep
            {
                Number = s.Number,
                Name = s.Name,
                Description = s.Description
            }).ToList()
        };

        return Ok(ApiResponse<PluginInfo>.Ok(info));
    }
}
