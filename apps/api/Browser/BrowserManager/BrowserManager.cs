using System.Collections.Concurrent;
using BrowserAgent.Api.Browser.Events;
using BrowserAgent.Api.Browser.Exceptions;
using BrowserAgent.Api.Browser.Interfaces;
using BrowserAgent.Api.Browser.Models;
using Microsoft.Playwright;

namespace BrowserAgent.Api.Browser.BrowserManager;

public class PlaywrightSessionManager : IBrowserManager, IAsyncDisposable
{
    private readonly ConcurrentDictionary<string, Session> _sessions = new();
    private readonly ILogger<PlaywrightSessionManager> _logger;
    private IPlaywright? _playwright;
    private bool _disposed;

    public PlaywrightSessionManager(ILogger<PlaywrightSessionManager> logger)
    {
        _logger = logger;
    }

    public async Task<BrowserInstance> LaunchAsync(BrowserOptions options, CancellationToken ct = default)
    {
        _playwright ??= await Microsoft.Playwright.Playwright.CreateAsync();
        ct.ThrowIfCancellationRequested();

        var browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = options.Headless,
            SlowMo = options.SlowMo,
            DownloadsPath = options.DownloadDirectory
        });

        var context = await browser.NewContextAsync(new BrowserNewContextOptions
        {
            ViewportSize = new ViewportSize { Width = options.ViewportWidth, Height = options.ViewportHeight },
            UserAgent = options.UserAgent,
            Locale = options.Locale,
            TimezoneId = options.Timezone,
            AcceptDownloads = true
        });

        var page = await context.NewPageAsync();
        var sessionId = Guid.NewGuid().ToString();

        page.Load += (_, _) =>
        {
            _logger.LogInformation("[{SessionId}] Page loaded: {Url}", sessionId, page.Url);
        };

        page.RequestFailed += (_, request) =>
        {
            _logger.LogWarning("[{SessionId}] Request failed: {Url} ({Error})",
                sessionId, request.Url, request.Failure);
        };

        var session = new Session
        {
            Browser = browser,
            Context = context,
            Page = page,
            Options = options,
            CreatedAt = DateTime.UtcNow
        };

        _sessions[sessionId] = session;

        _logger.LogInformation("Browser session started: {SessionId}", sessionId);

        return new BrowserInstance
        {
            SessionId = sessionId,
            BrowserType = "Chromium",
            CreatedAt = DateTime.UtcNow,
            CurrentUrl = page.Url,
            PageTitle = await page.TitleAsync()
        };
    }

    public async Task CloseAsync(string sessionId, CancellationToken ct = default)
    {
        if (_sessions.TryRemove(sessionId, out var session))
        {
            ct.ThrowIfCancellationRequested();
            try
            {
                await session.Page.CloseAsync();
                await session.Context.CloseAsync();
                await session.Browser.CloseAsync();
                _logger.LogInformation("Browser session closed: {SessionId}", sessionId);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error closing session {SessionId}", sessionId);
            }
        }
    }

    public Task<bool> IsActiveAsync(string sessionId, CancellationToken ct = default)
    {
        return Task.FromResult(_sessions.ContainsKey(sessionId));
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed) return;
        _disposed = true;

        foreach (var (id, session) in _sessions)
        {
            try
            {
                await session.Page.CloseAsync();
                await session.Context.CloseAsync();
                await session.Browser.CloseAsync();
            }
            catch { }
        }

        _sessions.Clear();
        _playwright?.Dispose();
    }

    internal Session? GetSession(string sessionId)
    {
        _sessions.TryGetValue(sessionId, out var session);
        return session;
    }

    internal int GetSessionCount() => _sessions.Count;

    internal class Session
    {
        public IBrowser Browser { get; init; } = null!;
        public IBrowserContext Context { get; init; } = null!;
        public IPage Page { get; init; } = null!;
        public BrowserOptions Options { get; init; } = null!;
        public DateTime CreatedAt { get; init; }
    }
}
