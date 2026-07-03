namespace BrowserAgent.Api.Browser.Events;

public enum BrowserEventType
{
    BrowserStarted,
    BrowserClosed,
    NavigationStarted,
    NavigationCompleted,
    PageLoaded,
    FormDetected,
    FormCompleted,
    ScreenshotTaken,
    HumanVerificationRequired,
    WorkflowPaused,
    WorkflowResumed,
    ErrorOccurred
}

public class BrowserEvent
{
    public BrowserEventType Type { get; init; }
    public string SessionId { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string? Url { get; init; }
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    public Dictionary<string, string> Metadata { get; init; } = new();
}
