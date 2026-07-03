using BrowserAgent.Api.Browser.BrowserManager;
using BrowserAgent.Api.Browser.Interfaces;
using BrowserAgent.Api.Browser.Utilities;
using BrowserAgent.Api.Browser.Exceptions;
using Microsoft.Playwright;

namespace BrowserAgent.Api.Browser.Navigation;

public class NavigationService : INavigationService
{
    private readonly PlaywrightSessionManager _browserManager;
    private readonly ILogger<NavigationService> _logger;

    public NavigationService(PlaywrightSessionManager browserManager, ILogger<NavigationService> logger)
    {
        _browserManager = browserManager;
        _logger = logger;
    }

    public async Task<NavigationResult> NavigateAsync(string sessionId, string url, CancellationToken ct = default)
    {
        var session = _browserManager.GetSession(sessionId);
        if (session is null)
            return Fail("SESSION_NOT_FOUND", "Browser session not found");

        ct.ThrowIfCancellationRequested();

        try
        {
            _logger.LogInformation("[{SessionId}] Navigating to: {Url}", sessionId, url);

            var response = await session.Page.GotoAsync(url, new PageGotoOptions
            {
                Timeout = 60_000,
                WaitUntil = WaitUntilState.NetworkIdle
            });

            return new NavigationResult
            {
                Success = response is not null,
                CurrentUrl = session.Page.Url,
                PageTitle = await session.Page.TitleAsync(),
                StatusCode = (int)(response?.Status ?? 0)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[{SessionId}] Navigation failed to: {Url}", sessionId, url);
            return Fail("NAVIGATION_FAILED", ex.Message);
        }
    }

    public async Task<NavigationResult> GoBackAsync(string sessionId, CancellationToken ct = default)
    {
        var session = _browserManager.GetSession(sessionId);
        if (session is null)
            return Fail("SESSION_NOT_FOUND", "Browser session not found");

        ct.ThrowIfCancellationRequested();

        try
        {
            var response = await session.Page.GoBackAsync(new PageGoBackOptions
            {
                WaitUntil = WaitUntilState.NetworkIdle,
                Timeout = 60_000
            });

            return new NavigationResult
            {
                Success = true,
                CurrentUrl = session.Page.Url,
                PageTitle = await session.Page.TitleAsync()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[{SessionId}] Go back failed", sessionId);
            return Fail("NAVIGATION_FAILED", ex.Message);
        }
    }

    public async Task<NavigationResult> RefreshAsync(string sessionId, CancellationToken ct = default)
    {
        var session = _browserManager.GetSession(sessionId);
        if (session is null)
            return Fail("SESSION_NOT_FOUND", "Browser session not found");

        ct.ThrowIfCancellationRequested();

        try
        {
            var response = await session.Page.ReloadAsync(new PageReloadOptions
            {
                WaitUntil = WaitUntilState.NetworkIdle,
                Timeout = 60_000
            });

            return new NavigationResult
            {
                Success = true,
                CurrentUrl = session.Page.Url,
                PageTitle = await session.Page.TitleAsync()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[{SessionId}] Refresh failed", sessionId);
            return Fail("NAVIGATION_FAILED", ex.Message);
        }
    }

    private static NavigationResult Fail(string code, string message) => new()
    {
        Success = false,
        Error = $"{code}: {message}"
    };
}
