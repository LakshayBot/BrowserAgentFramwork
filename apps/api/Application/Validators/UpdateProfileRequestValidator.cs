using BrowserAgent.Api.Application.DTOs.Profile;
using FluentValidation;

namespace BrowserAgent.Api.Application.Validators;

public class UpdateProfileRequestValidator : AbstractValidator<UpdateProfileRequest>
{
    public UpdateProfileRequestValidator()
    {
        RuleFor(x => x.FirstName)
            .MaximumLength(100);

        RuleFor(x => x.LastName)
            .MaximumLength(100);

        RuleFor(x => x.Phone)
            .MaximumLength(30);

        When(x => !string.IsNullOrEmpty(x.Phone), () =>
        {
            RuleFor(x => x.Phone!)
                .Matches(@"^[\+\d\s\-\(\)]+$").WithMessage("Invalid phone number format");
        });

        RuleFor(x => x.Location)
            .MaximumLength(200);

        RuleFor(x => x.LinkedIn)
            .MaximumLength(500);

        RuleFor(x => x.GitHub)
            .MaximumLength(500);

        RuleFor(x => x.Portfolio)
            .MaximumLength(500);

        RuleFor(x => x.Website)
            .MaximumLength(500);

        RuleFor(x => x.Summary)
            .MaximumLength(2000);
    }
}
