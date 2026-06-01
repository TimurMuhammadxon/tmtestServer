using FluentValidation;

namespace OnlineTesting.Application.Users.Commands.SetCredentials;

public class SetCredentialsCommandValidator : AbstractValidator<SetCredentialsCommand>
{
    public SetCredentialsCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(256)
            .Must(e => !e.EndsWith("@telegram.local", StringComparison.OrdinalIgnoreCase))
            .WithMessage("Please provide a real email address.");

        RuleFor(x => x.Password)
            .NotEmpty()
            .MinimumLength(6)
            .MaximumLength(100);
    }
}
