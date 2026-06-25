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
            .MinimumLength(6).WithMessage("Password must be at least 6 characters.")
            .MaximumLength(128);
    }

    private static bool NotUseReservedDomain(string? email)
    {
        if (string.IsNullOrWhiteSpace(email)) return true; // другие правила отловят
        return !email.Trim().ToLowerInvariant().EndsWith(ReservedDomain);
    }
}