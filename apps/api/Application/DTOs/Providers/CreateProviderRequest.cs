using BrowserAgent.Api.Domain.Enums;

namespace BrowserAgent.Api.Application.DTOs.Providers;

public class CreateProviderRequest
{
    public ProviderType ProviderType { get; set; }
    public string ModelName { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public string? BaseUrl { get; set; }
    public double Temperature { get; set; } = 0.7;
    public int MaxTokens { get; set; } = 4096;
    public bool IsDefault { get; set; }
}
