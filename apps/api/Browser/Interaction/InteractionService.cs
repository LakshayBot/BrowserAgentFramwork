using BrowserAgent.Api.Browser.BrowserManager;
using BrowserAgent.Api.Browser.Interfaces;
using BrowserAgent.Api.Browser.Utilities;
using BrowserAgent.Api.Browser.Exceptions;
using Microsoft.Playwright;

namespace BrowserAgent.Api.Browser.Interaction;

public class InteractionService : IInteractionService
{
    private readonly PlaywrightSessionManager _browserManager;
    private readonly ILogger<InteractionService> _logger;

    public InteractionService(PlaywrightSessionManager browserManager, ILogger<InteractionService> logger)
    {
        _browserManager = browserManager;
        _logger = logger;
    }

    public async Task<InteractionResult> ClickAsync(string sessionId, string selector, CancellationToken ct = default)
    {
        return await ExecuteAsync<object>(sessionId, "Click", selector, async page =>
        {
            await page.Locator(selector).WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
            await page.Locator(selector).ClickAsync();
        }, ct);
    }

    public async Task<InteractionResult> TypeAsync(string sessionId, string selector, string text, CancellationToken ct = default)
    {
        return await ExecuteAsync<object>(sessionId, "Type", selector, async page =>
        {
            var locator = page.Locator(selector);
            await locator.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
            await locator.ClearAsync();
            await locator.FillAsync(text);
        }, ct);
    }

    public async Task<InteractionResult> SelectAsync(string sessionId, string selector, string value, CancellationToken ct = default)
    {
        return await ExecuteAsync<object>(sessionId, "Select", selector, async page =>
        {
            var locator = page.Locator(selector);
            await locator.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
            await locator.SelectOptionAsync(new[] { value });
        }, ct);
    }

    public async Task<InteractionResult> CheckAsync(string sessionId, string selector, CancellationToken ct = default)
    {
        return await ExecuteAsync<object>(sessionId, "Check", selector, async page =>
        {
            var locator = page.Locator(selector);
            await locator.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
            await locator.CheckAsync();
        }, ct);
    }

    public async Task<InteractionResult> UncheckAsync(string sessionId, string selector, CancellationToken ct = default)
    {
        return await ExecuteAsync<object>(sessionId, "Uncheck", selector, async page =>
        {
            var locator = page.Locator(selector);
            await locator.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
            await locator.UncheckAsync();
        }, ct);
    }

    public async Task<InteractionResult> UploadFileAsync(string sessionId, string selector, string filePath, CancellationToken ct = default)
    {
        return await ExecuteAsync<object>(sessionId, "Upload", selector, async page =>
        {
            var locator = page.Locator(selector);
            await locator.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Attached });
            await locator.SetInputFilesAsync(filePath);
        }, ct);
    }

    public async Task<InteractionResult> HoverAsync(string sessionId, string selector, CancellationToken ct = default)
    {
        return await ExecuteAsync<object>(sessionId, "Hover", selector, async page =>
        {
            var locator = page.Locator(selector);
            await locator.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
            await locator.HoverAsync();
        }, ct);
    }

    private async Task<InteractionResult> ExecuteAsync<T>(
        string sessionId, string action, string selector,
        Func<IPage, Task> actionFunc, CancellationToken ct)
    {
        var session = _browserManager.GetSession(sessionId);
        if (session is null)
            return Fail("SESSION_NOT_FOUND", "Browser session not found");

        ct.ThrowIfCancellationRequested();

        try
        {
            await RetryStrategy.ExecuteAsync(
                () => actionFunc(session.Page), ct: ct);

            _logger.LogInformation("[{SessionId}] {Action} on '{Selector}' succeeded",
                sessionId, action, selector);

            return new InteractionResult
            {
                Success = true,
                Action = action,
                Error = null
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[{SessionId}] {Action} on '{Selector}' failed",
                sessionId, action, selector);

            return Fail(action, ex.Message);
        }
    }

    private static InteractionResult Fail(string action, string error) => new()
    {
        Success = false,
        Action = action,
        Error = error
    };
}
