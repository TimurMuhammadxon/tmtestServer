using FluentValidation;

namespace OnlineTesting.Application.Auth.Commands.Register;

public class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    private const string ReservedDomain = "@telegram.local";

    public RegisterCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(256)
            .Must(NotUseReservedDomain)
                .WithMessage("This email domain is reserved.");

        RuleFor(x => x.Password)
            .NotEmpty()
            .MinimumLength(8)
            .Matches("[A-Za-z]").WithMessage("Password must contain at least one letter.")
            .Matches("[0-9]").WithMessage("Password must contain at least one digit.")
            .MaximumLength(128);
    }

    private static bool NotUseReservedDomain(string? email)
    {
        if (string.IsNullOrWhiteSpace(email)) return true; // другие правила отловят
        return !email.Trim().ToLowerInvariant().EndsWith(ReservedDomain);
    }
}