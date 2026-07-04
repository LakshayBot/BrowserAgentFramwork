using System.Net.Http.Json;
using System.Text.Json;
using BrowserAgent.Api.Application.DTOs.Providers;
using BrowserAgent.Api.Application.Interfaces;
using BrowserAgent.Api.Domain.Entities;
using BrowserAgent.Api.Domain.Enums;
using BrowserAgent.Api.Domain.Exceptions;
using BrowserAgent.Api.Infrastructure.Data;
using BrowserAgent.Api.Infrastructure.Encryption;
using Microsoft.EntityFrameworkCore;

namespace BrowserAgent.Api.Infrastructure.Services;

public class ProviderService : IProviderService
{
    private readonly AppDbContext _context;
    private readonly IEncryptionService _encryption;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<ProviderService> _logger;

    public ProviderService(
        AppDbContext context,
        IEncryptionService encryption,
        IHttpClientFactory httpClientFactory,
        ILogger<ProviderService> logger)
    {
        _context = context;
        _encryption = encryption;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task<List<ProviderDto>> GetAllAsync(Guid userId, CancellationToken ct = default)
    {
        var configs = await _context.ProviderConfigs
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.IsDefault)
            .ThenByDescending(x => x.CreatedAt)
            .ToListAsync(ct);

        return configs.Select(MapToDto).ToList();
    }

    public async Task<ProviderDto> GetByIdAsync(Guid id, Guid userId, CancellationToken ct = default)
    {
        var config = await GetOwnedConfigAsync(id, userId, ct);
        return MapToDto(config);
    }

    public async Task<ProviderDto> CreateAsync(Guid userId, CreateProviderRequest request, CancellationToken ct = default)
    {
        var existingCount = await _context.ProviderConfigs
            .CountAsync(x => x.UserId == userId, ct);

        if (existingCount >= 10)
        {
            throw new DomainException("MAX_PROVIDERS", "Maximum of 10 provider configurations per user");
        }

        if (request.IsDefault)
        {
            await ClearDefaultAsync(userId, ct);
        }

        var config = new ProviderConfig
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            ProviderType = request.ProviderType,
            ModelName = request.ModelName.Trim(),
            EncryptedApiKey = _encryption.Encrypt(request.ApiKey),
            BaseUrl = request.BaseUrl?.Trim(),
            Temperature = request.Temperature,
            MaxTokens = request.MaxTokens,
            IsDefault = request.IsDefault || existingCount == 0,
            CreatedAt = DateTime.UtcNow
        };

        _context.ProviderConfigs.Add(config);
        await _context.SaveChangesAsync(ct);

        _logger.LogInformation("Provider created: {Id} {Type} for user {UserId}",
            config.Id, config.ProviderType, userId);

        return MapToDto(config);
    }

    public async Task<ProviderDto> UpdateAsync(Guid id, Guid userId, UpdateProviderRequest request, CancellationToken ct = default)
    {
        var config = await GetOwnedConfigAsync(id, userId, ct);

        config.ProviderType = request.ProviderType;
        config.ModelName = request.ModelName.Trim();
        config.BaseUrl = request.BaseUrl?.Trim();
        config.Temperature = request.Temperature;
        config.MaxTokens = request.MaxTokens;

        if (!string.IsNullOrEmpty(request.ApiKey))
        {
            config.EncryptedApiKey = _encryption.Encrypt(request.ApiKey);
        }

        await _context.SaveChangesAsync(ct);

        _logger.LogInformation("Provider updated: {Id} for user {UserId}", config.Id, userId);

        return MapToDto(config);
    }

    public async Task DeleteAsync(Guid id, Guid userId, CancellationToken ct = default)
    {
        var config = await GetOwnedConfigAsync(id, userId, ct);
        _context.ProviderConfigs.Remove(config);
        await _context.SaveChangesAsync(ct);

        _logger.LogInformation("Provider deleted: {Id} for user {UserId}", id, userId);
    }

    public async Task<ProviderValidationResult> ValidateAsync(Guid id, Guid userId, CancellationToken ct = default)
    {
        var config = await GetOwnedConfigAsync(id, userId, ct);
        var apiKey = _encryption.Decrypt(config.EncryptedApiKey);

        try
        {
            return config.ProviderType switch
            {
                ProviderType.DeepSeek => await ValidateDeepSeekAsync(config, apiKey, ct),
                ProviderType.Ollama => await ValidateOllamaAsync(config, ct),
                _ => new ProviderValidationResult { Success = false, Message = "Unsupported provider" }
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Provider validation failed: {Id}", id);
            return new ProviderValidationResult
            {
                Success = false,
                Message = $"Validation failed: {ex.Message}"
            };
        }
    }

    public async Task<ProviderDto> SetDefaultAsync(Guid id, Guid userId, CancellationToken ct = default)
    {
        var config = await GetOwnedConfigAsync(id, userId, ct);

        if (config.IsDefault)
        {
            return MapToDto(config);
        }

        await ClearDefaultAsync(userId, ct);

        config.IsDefault = true;
        await _context.SaveChangesAsync(ct);

        _logger.LogInformation("Default provider set: {Id} for user {UserId}", id, userId);

        return MapToDto(config);
    }

    private async Task ClearDefaultAsync(Guid userId, CancellationToken ct)
    {
        var currentDefault = await _context.ProviderConfigs
            .FirstOrDefaultAsync(x => x.UserId == userId && x.IsDefault, ct);

        if (currentDefault is not null)
        {
            currentDefault.IsDefault = false;
        }
    }

    private async Task<ProviderConfig> GetOwnedConfigAsync(Guid id, Guid userId, CancellationToken ct)
    {
        var config = await _context.ProviderConfigs
            .FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId, ct);

        if (config is null)
        {
            throw new NotFoundException("ProviderConfig", id);
        }

        return config;
    }

    private async Task<ProviderValidationResult> ValidateDeepSeekAsync(
        ProviderConfig config, string apiKey, CancellationToken ct)
    {
        var baseUrl = (config.BaseUrl ?? "https://api.deepseek.com").TrimEnd('/');
        var client = _httpClientFactory.CreateClient("DeepSeek");

        var request = new HttpRequestMessage(HttpMethod.Post, $"{baseUrl}/v1/chat/completions");
        request.Headers.Add("Authorization", $"Bearer {apiKey}");

        request.Content = JsonContent.Create(new
        {
            model = config.ModelName,
            messages = new[] { new { role = "user", content = "Respond with the word 'ok'" } },
            max_tokens = 10
        });

        var response = await client.SendAsync(request, ct);
        var body = await response.Content.ReadAsStringAsync(ct);

        if (!response.IsSuccessStatusCode)
        {
            return new ProviderValidationResult
            {
                Success = false,
                Message = $"API error ({response.StatusCode}): {TryExtractErrorMessage(body)}"
            };
        }

        return new ProviderValidationResult
        {
            Success = true,
            Message = "Connection successful",
            Model = config.ModelName
        };
    }

    private async Task<ProviderValidationResult> ValidateOllamaAsync(
        ProviderConfig config, CancellationToken ct)
    {
        var baseUrl = (config.BaseUrl ?? "http://localhost:11434").TrimEnd('/');
        var client = _httpClientFactory.CreateClient("Ollama");

        var request = new HttpRequestMessage(HttpMethod.Get, $"{baseUrl}/api/tags");
        var response = await client.SendAsync(request, ct);

        if (!response.IsSuccessStatusCode)
        {
            return new ProviderValidationResult
            {
                Success = false,
                Message = $"Ollama returned status {response.StatusCode}"
            };
        }

        var body = await response.Content.ReadAsStringAsync(ct);

        return new ProviderValidationResult
        {
            Success = true,
            Message = "Ollama is reachable",
            Model = config.ModelName
        };
    }

    private static string TryExtractErrorMessage(string body)
    {
        try
        {
            using var doc = JsonDocument.Parse(body);
            if (doc.RootElement.TryGetProperty("error", out var error))
            {
                return error.GetString() ?? "Unknown error";
            }
            if (doc.RootElement.TryGetProperty("message", out var message))
            {
                return message.GetString() ?? "Unknown error";
            }
        }
        catch { }

        return body.Length > 200 ? body[..200] + "..." : body;
    }

    private ProviderDto MapToDto(ProviderConfig config)
    {
        string apiKey;
        try { apiKey = _encryption.Decrypt(config.EncryptedApiKey); }
        catch { apiKey = string.Empty; }

        return new ProviderDto
        {
            Id = config.Id,
            ProviderType = config.ProviderType.ToString(),
            ModelName = config.ModelName,
            BaseUrl = config.BaseUrl,
            Temperature = config.Temperature,
            MaxTokens = config.MaxTokens,
            IsDefault = config.IsDefault,
            ApiKey = apiKey,
            CreatedAt = config.CreatedAt
        };
    }
}
