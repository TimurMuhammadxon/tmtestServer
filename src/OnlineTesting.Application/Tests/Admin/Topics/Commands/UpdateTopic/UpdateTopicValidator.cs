using FluentValidation;

namespace OnlineTesting.Application.Tests.Admin.Topics.Commands.UpdateTopic;

public class UpdateTopicValidator : AbstractValidator<UpdateTopicCommand>
{
    public UpdateTopicValidator()
    {
        RuleFor(x => x.Id).NotEmpty();

        RuleFor(x => x.Code)
            .NotEmpty().Length(2, 50)
            .Matches("^[a-z0-9-]+$")
                .WithMessage("Code must be lowercase letters, digits, hyphens.");

        RuleFor(x => x.OrderIndex).GreaterThanOrEqualTo(0);
    }
}