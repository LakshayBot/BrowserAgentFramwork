using BrowserAgent.Api.Domain.Enums;

namespace BrowserAgent.Api.Domain.Entities;

public class ProviderConfig
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public ProviderType ProviderType { get; set; }
    public string ModelName { get; set; } = string.Empty;
    public string EncryptedApiKey { get; set; } = string.Empty;
    public string? BaseUrl { get; set; }
    public double Temperature { get; set; } = 0.0;
    public int MaxTokens { get; set; } = 4096;
    public bool IsDefault { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }

    public User User { get; set; } = null!;
}
