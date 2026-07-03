namespace BrowserAgent.Api.Browser.Models;

public class BrowserSessionInfo
{
    public string SessionId { get; set; } = string.Empty;
    public string BrowserType { get; set; } = "Chromium";
    public string? CurrentUrl { get; set; }
    public string? PageTitle { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; }
}
