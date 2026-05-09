using FluentValidation;

namespace OnlineTesting.Application.Tests.Admin.Topics.Commands.SetTopicActive;

public class SetTopicActiveValidator : AbstractValidator<SetTopicActiveCommand>
{
    public SetTopicActiveValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}