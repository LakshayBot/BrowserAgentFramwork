namespace BrowserAgent.Api.Browser.Models;

public class BrowserInstance
{
    public string SessionId { get; set; } = string.Empty;
    public string BrowserType { get; set; } = "Chromium";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? CurrentUrl { get; set; }
    public string? PageTitle { get; set; }
}
