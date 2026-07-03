using BrowserAgent.Api.Application.DTOs.Documents;
using Microsoft.AspNetCore.Http;

namespace BrowserAgent.Api.Application.Interfaces;

public interface IDocumentService
{
    Task<List<DocumentDto>> GetAllAsync(Guid userId, CancellationToken ct = default);
    Task<DocumentDto> GetByIdAsync(Guid id, Guid userId, CancellationToken ct = default);
    Task<DocumentUploadResult> UploadAsync(Guid userId, IFormFile file, string documentType, CancellationToken ct = default);
    Task DeleteAsync(Guid id, Guid userId, CancellationToken ct = default);
}
