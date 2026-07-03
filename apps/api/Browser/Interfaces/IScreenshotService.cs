namespace BrowserAgent.Api.Browser.Interfaces;

public interface IScreenshotService
{
    Task<string> CaptureAsync(string sessionId, string? fileName = null, CancellationToken ct = default);
    Task<string> CaptureFullPageAsync(string sessionId, string? fileName = null, CancellationToken ct = default);
}
