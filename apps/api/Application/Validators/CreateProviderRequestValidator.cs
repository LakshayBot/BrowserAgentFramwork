using BrowserAgent.Api.Application.DTOs.Providers;
using BrowserAgent.Api.Domain.Enums;
using FluentValidation;

namespace BrowserAgent.Api.Application.Validators;

public class CreateProviderRequestValidator : AbstractValidator<CreateProviderRequest>
{
    public CreateProviderRequestValidator()
    {
        RuleFor(x => x.ProviderType)
            .IsInEnum().WithMessage("Invalid provider type. Supported: DeepSeek, DeepSeekFlash, OpenAI, Ollama");

        RuleFor(x => x.ModelName)
            .NotEmpty().WithMessage("Model name is required")
            .MaximumLength(100);

        RuleFor(x => x.ApiKey)
            .MaximumLength(4000);

        When(x => x.ProviderType != ProviderType.Ollama, () =>
        {
            RuleFor(x => x.ApiKey)
                .NotEmpty().WithMessage("API key is required for this provider");
        });

        RuleFor(x => x.BaseUrl)
            .MaximumLength(500)
            .Must(x => string.IsNullOrEmpty(x) || Uri.TryCreate(x, UriKind.Absolute, out _))
            .WithMessage("Base URL must be a valid absolute URL");

        RuleFor(x => x.Temperature)
            .InclusiveBetween(0.0, 2.0).WithMessage("Temperature must be between 0.0 and 2.0");

        RuleFor(x => x.MaxTokens)
            .InclusiveBetween(1, 100_000).WithMessage("Max tokens must be between 1 and 100,000");
    }
}
