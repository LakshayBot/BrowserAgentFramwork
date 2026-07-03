using BrowserAgent.Api.Application.DTOs.Workflows;
using FluentValidation;

namespace BrowserAgent.Api.Application.Validators;

public class CreateWorkflowRequestValidator : AbstractValidator<CreateWorkflowRequest>
{
    public CreateWorkflowRequestValidator()
    {
        RuleFor(x => x.Url)
            .NotEmpty().WithMessage("URL is required")
            .MaximumLength(2048)
            .Must(url => Uri.TryCreate(url, UriKind.Absolute, out var uri)
                && (uri.Scheme == "http" || uri.Scheme == "https"))
            .WithMessage("URL must be a valid HTTP or HTTPS URL");

        RuleFor(x => x.PluginName)
            .MaximumLength(100);
    }
}
