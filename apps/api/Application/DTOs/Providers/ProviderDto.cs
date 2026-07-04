using BrowserAgent.Api.Domain.Enums;

namespace BrowserAgent.Api.Application.DTOs.Providers;

public class ProviderDto
{
    public Guid Id { get; set; }
    public string ProviderType { get; set; } = string.Empty;
    public string ModelName { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public string? BaseUrl { get; set; }
    public double Temperature { get; set; }
    public int MaxTokens { get; set; }
    public bool IsDefault { get; set; }
    public DateTime CreatedAt { get; set; }
}
