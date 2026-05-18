using FluentValidation;

namespace OnlineTesting.Application.TestLinks.Commands.StartTestLink;

public class StartTestLinkValidator : AbstractValidator<StartTestLinkCommand>
{
    public StartTestLinkValidator()
    {
        RuleFor(x => x.Code).NotEmpty().MaximumLength(10);
    }
}
