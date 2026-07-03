namespace BrowserAgent.Api.Browser.Utilities;

public static class RetryStrategy
{
    public static int MaxRetries { get; set; } = 3;

    public static async Task ExecuteAsync(
        Func<Task> action,
        Func<Exception, bool>? shouldRetry = null,
        CancellationToken ct = default)
    {
        var retryCount = 0;
        var delay = TimeSpan.FromMilliseconds(500);

        while (true)
        {
            try
            {
                await action();
                return;
            }
            catch (Exception ex) when (retryCount < MaxRetries && (shouldRetry?.Invoke(ex) ?? IsRecoverable(ex)))
            {
                retryCount++;
                ct.ThrowIfCancellationRequested();
                await Task.Delay(delay, ct);
                delay = TimeSpan.FromMilliseconds(delay.TotalMilliseconds * 2);
            }
        }
    }

    public static bool IsRecoverable(Exception ex)
    {
        return ex switch
        {
            Microsoft.Playwright.PlaywrightException pex
                when pex.Message.Contains("detached") || pex.Message.Contains("timeout") || pex.Message.Contains("intercepted") => true,
            _ => false
        };
    }
}
