namespace BrowserAgent.Api.Browser.Models;

public class BrowserState
{
    public string? CurrentUrl { get; set; }
    public string? PageTitle { get; set; }
    public string? HtmlSnapshot { get; set; }
    public string? ScreenshotBase64 { get; set; }
}
