namespace BrowserAgent.Api.Application.DTOs.Documents;

public class DocumentUploadResult
{
    public Guid Id { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public string MimeType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string Sha256 { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
