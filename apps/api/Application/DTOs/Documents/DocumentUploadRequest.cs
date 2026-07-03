using Microsoft.AspNetCore.Http;

namespace BrowserAgent.Api.Application.DTOs.Documents;

public class DocumentUploadRequest
{
    public IFormFile File { get; set; } = null!;
    public string DocumentType { get; set; } = string.Empty;
}
