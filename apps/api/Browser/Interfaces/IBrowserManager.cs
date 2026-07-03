using BrowserAgent.Api.Browser.Models;

namespace BrowserAgent.Api.Browser.Interfaces;

public interface IBrowserManager
{
    Task<BrowserInstance> LaunchAsync(BrowserOptions options, CancellationToken ct = default);
    Task CloseAsync(string sessionId, CancellationToken ct = default);
    Task<bool> IsActiveAsync(string sessionId, CancellationToken ct = default);
}

public class BrowserOptions
{
    public bool Headless { get; set; } = true;
    public int? SlowMo { get; set; }
    public string? UserAgent { get; set; }
    public int ViewportWidth { get; set; } = 1280;
    public int ViewportHeight { get; set; } = 720;
    public string Locale { get; set; } = "en-US";
    public string Timezone { get; set; } = "UTC";
    public string? DownloadDirectory { get; set; }
}
