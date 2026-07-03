namespace BrowserAgent.Api.Infrastructure.Storage;

public interface IStorageService
{
    Task<string> SaveAsync(string path, Stream content, CancellationToken ct = default);
    Task<Stream> GetAsync(string path, CancellationToken ct = default);
    Task DeleteAsync(string path, CancellationToken ct = default);
    Task<bool> ExistsAsync(string path, CancellationToken ct = default);
}
