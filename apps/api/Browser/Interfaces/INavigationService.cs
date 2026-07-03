namespace BrowserAgent.Api.Browser.Interfaces;

public interface INavigationService
{
    Task<NavigationResult> NavigateAsync(string sessionId, string url, CancellationToken ct = default);
    Task<NavigationResult> GoBackAsync(string sessionId, CancellationToken ct = default);
    Task<NavigationResult> RefreshAsync(string sessionId, CancellationToken ct = default);
}

public class NavigationResult
{
    public bool Success { get; set; }
    public string? Error { get; set; }
    public string? CurrentUrl { get; set; }
    public string? PageTitle { get; set; }
    public int StatusCode { get; set; }
}
