using BrowserAgent.Api.Browser.Interfaces;
using BrowserAgent.Api.Browser.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BrowserAgent.Api.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/browser")]
public class BrowserController : ControllerBase
{
    private readonly IBrowserManager _browserManager;
    private readonly INavigationService _navigation;
    private readonly IFormExtractor _extractor;
    private readonly IInteractionService _interaction;
    private readonly IScreenshotService _screenshots;
    private readonly ILogger<BrowserController> _logger;

    public BrowserController(
        IBrowserManager browserManager,
        INavigationService navigation,
        IFormExtractor extractor,
        IInteractionService interaction,
        IScreenshotService screenshots,
        ILogger<BrowserController> logger)
    {
        _browserManager = browserManager;
        _navigation = navigation;
        _extractor = extractor;
        _interaction = interaction;
        _screenshots = screenshots;
        _logger = logger;
    }

    [HttpPost("start")]
    public async Task<IActionResult> Start([FromBody] BrowserOptions? options = null, CancellationToken ct = default)
    {
        var instance = await _browserManager.LaunchAsync(options ?? new BrowserOptions(), ct);
        return Ok(ApiResponse<BrowserInstance>.Ok(instance));
    }

    [HttpPost("{sessionId}/navigate")]
    public async Task<IActionResult> Navigate(string sessionId, [FromBody] NavigateRequest request, CancellationToken ct)
    {
        var userId = User.GetUserId();
        var result = await _navigation.NavigateAsync(sessionId, request.Url, ct);
        return Ok(ApiResponse<NavigationResult>.Ok(result));
    }

    [HttpGet("{sessionId}/page")]
    public async Task<IActionResult> ExtractPage(string sessionId, CancellationToken ct)
    {
        var result = await _extractor.ExtractPageAsync(sessionId, ct);
        return Ok(ApiResponse<ExtractedPage>.Ok(result));
    }

    [HttpGet("{sessionId}/screenshot")]
    public async Task<IActionResult> Screenshot(string sessionId, CancellationToken ct)
    {
        var path = await _screenshots.CaptureAsync(sessionId, ct: ct);
        return Ok(ApiResponse<object>.Ok(new { path }));
    }

    [HttpPost("{sessionId}/click")]
    public async Task<IActionResult> Click(string sessionId, [FromBody] SelectorRequest request, CancellationToken ct)
    {
        var result = await _interaction.ClickAsync(sessionId, request.Selector, ct);
        return Ok(ApiResponse<InteractionResult>.Ok(result));
    }

    [HttpPost("{sessionId}/type")]
    public async Task<IActionResult> Type(string sessionId, [FromBody] TypeRequest request, CancellationToken ct)
    {
        var result = await _interaction.TypeAsync(sessionId, request.Selector, request.Text, ct);
        return Ok(ApiResponse<InteractionResult>.Ok(result));
    }

    [HttpPost("{sessionId}/close")]
    public async Task<IActionResult> Close(string sessionId, CancellationToken ct)
    {
        await _browserManager.CloseAsync(sessionId, ct);
        return NoContent();
    }
}

public class NavigateRequest
{
    public string Url { get; set; } = string.Empty;
}

public class SelectorRequest
{
    public string Selector { get; set; } = string.Empty;
}

public class TypeRequest
{
    public string Selector { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
}
