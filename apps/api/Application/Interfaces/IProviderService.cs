using BrowserAgent.Api.Application.DTOs.Providers;

namespace BrowserAgent.Api.Application.Interfaces;

public interface IProviderService
{
    Task<List<ProviderDto>> GetAllAsync(Guid userId, CancellationToken ct = default);
    Task<ProviderDto> GetByIdAsync(Guid id, Guid userId, CancellationToken ct = default);
    Task<ProviderDto> CreateAsync(Guid userId, CreateProviderRequest request, CancellationToken ct = default);
    Task<ProviderDto> UpdateAsync(Guid id, Guid userId, UpdateProviderRequest request, CancellationToken ct = default);
    Task DeleteAsync(Guid id, Guid userId, CancellationToken ct = default);
    Task<ProviderValidationResult> ValidateAsync(Guid id, Guid userId, CancellationToken ct = default);
    Task<ProviderDto> SetDefaultAsync(Guid id, Guid userId, CancellationToken ct = default);
}
