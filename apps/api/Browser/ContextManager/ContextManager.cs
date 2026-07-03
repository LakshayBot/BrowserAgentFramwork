
using BrowserAgent.Api.Browser.BrowserManager;
using BrowserAgent.Api.Browser.Interfaces;
using BrowserAgent.Api.Browser.Models;
using Microsoft.Playwright;

namespace BrowserAgent.Api.Browser.ContextManager;

public class ContextManager
{
    private readonly PlaywrightSessionManager _browserManager;
    private readonly ILogger<ContextManager> _logger;

    public ContextManager(PlaywrightSessionManager browserManager, ILogger<ContextManager> logger)
    {
        _browserManager = browserManager;
        _logger = logger;
    }

    public async Task<IPage> GetPageAsync(string sessionId, CancellationToken ct = default)
    {
        var session = _browserManager.GetSession(sessionId)
            ?? throw new Exceptions.BrowserException("SESSION_NOT_FOUND", $"Browser session {sessionId} not found");

        ct.ThrowIfCancellationRequested();

        if (session.Page.IsClosed)
        {
            _logger.LogInformation("[{SessionId}] Page was closed, creating new page", sessionId);
            var newPage = await session.Context.NewPageAsync();
            return newPage;
        }

        return session.Page;
    }

    public bool IsSessionActive(string sessionId)
    {
        return _browserManager.GetSession(sessionId) != null;
    }

    public int SessionCount => _browserManager.GetSessionCount();
}
