
using BrowserAgent.Api.Browser.BrowserManager;
using BrowserAgent.Api.Browser.Interfaces;

namespace BrowserAgent.Api.Browser.Screenshot;

public class ScreenshotService : IScreenshotService
{
    private readonly PlaywrightSessionManager _browserManager;
    private readonly string _storagePath;
    private readonly ILogger<ScreenshotService> _logger;

    public ScreenshotService(PlaywrightSessionManager browserManager, IConfiguration configuration, ILogger<ScreenshotService> logger)
    {
        _browserManager = browserManager;
        _storagePath = configuration["Storage:LocalPath"] ?? Path.Combine(Directory.GetCurrentDirectory(), "storage");
        _logger = logger;
    }

    public async Task<string> CaptureAsync(string sessionId, string? fileName = null, CancellationToken ct = default)
    {
        var session = _browserManager.GetSession(sessionId);
        if (session is null)
            throw new Exceptions.BrowserException("SESSION_NOT_FOUND", "Browser session not found");

        ct.ThrowIfCancellationRequested();

        var screenshotDir = Path.Combine(_storagePath, "screenshots");
        Directory.CreateDirectory(screenshotDir);

        var name = fileName ?? $"screenshot_{sessionId}_{DateTime.UtcNow:yyyyMMddHHmmss}.png";
        var path = Path.Combine(screenshotDir, name);

        await session.Page.ScreenshotAsync(new Microsoft.Playwright.PageScreenshotOptions
        {
            Path = path,
            Type = Microsoft.Playwright.ScreenshotType.Png
        });

        _logger.LogInformation("[{SessionId}] Screenshot saved: {Path}", sessionId, path);
        return path;
    }

    public async Task<string> CaptureFullPageAsync(string sessionId, string? fileName = null, CancellationToken ct = default)
    {
        var session = _browserManager.GetSession(sessionId);
        if (session is null)
            throw new Exceptions.BrowserException("SESSION_NOT_FOUND", "Browser session not found");

        ct.ThrowIfCancellationRequested();

        var screenshotDir = Path.Combine(_storagePath, "screenshots");
        Directory.CreateDirectory(screenshotDir);

        var name = fileName ?? $"fullpage_{sessionId}_{DateTime.UtcNow:yyyyMMddHHmmss}.png";
        var path = Path.Combine(screenshotDir, name);

        await session.Page.ScreenshotAsync(new Microsoft.Playwright.PageScreenshotOptions
        {
            Path = path,
            FullPage = true,
            Type = Microsoft.Playwright.ScreenshotType.Png
        });

        _logger.LogInformation("[{SessionId}] Full-page screenshot saved: {Path}", sessionId, path);
        return path;
    }
}
