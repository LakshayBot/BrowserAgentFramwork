namespace BrowserAgent.Api.Browser.Interfaces;

public interface IInteractionService
{
    Task<InteractionResult> ClickAsync(string sessionId, string selector, CancellationToken ct = default);
    Task<InteractionResult> TypeAsync(string sessionId, string selector, string text, CancellationToken ct = default);
    Task<InteractionResult> SelectAsync(string sessionId, string selector, string value, CancellationToken ct = default);
    Task<InteractionResult> CheckAsync(string sessionId, string selector, CancellationToken ct = default);
    Task<InteractionResult> UncheckAsync(string sessionId, string selector, CancellationToken ct = default);
    Task<InteractionResult> UploadFileAsync(string sessionId, string selector, string filePath, CancellationToken ct = default);
    Task<InteractionResult> HoverAsync(string sessionId, string selector, CancellationToken ct = default);
}

public class InteractionResult
{
    public bool Success { get; set; }
    public string? Error { get; set; }
    public string? Action { get; set; }
}
