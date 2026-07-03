using BrowserAgent.Api.Domain.Enums;

namespace BrowserAgent.Api.Application.DTOs.Providers;

public class UpdateProviderRequest
{
    public ProviderType ProviderType { get; set; }
    public string ModelName { get; set; } = string.Empty;
    public string? ApiKey { get; set; }
    public string? BaseUrl { get; set; }
    public double Temperature { get; set; } = 0.7;
    public int MaxTokens { get; set; } = 4096;
}
