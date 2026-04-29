using FluentValidation;

namespace OnlineTesting.Application.Auth.Commands.TelegramLogin;

public class TelegramLoginCommandValidator : AbstractValidator<TelegramLoginCommand>
{
    public TelegramLoginCommandValidator()
    {
        RuleFor(x => x.InitData)
            .NotEmpty()
            .MaximumLength(8192); // защита от слишком больших payload
    }
}