namespace BrowserAgent.Api.Application.DTOs.Providers;

public class ProviderValidationResult
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public string? Model { get; set; }
}
