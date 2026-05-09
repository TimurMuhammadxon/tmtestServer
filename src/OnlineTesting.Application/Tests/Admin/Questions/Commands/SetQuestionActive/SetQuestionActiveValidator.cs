using FluentValidation;

namespace OnlineTesting.Application.Tests.Admin.Questions.Commands.SetQuestionActive;

public class SetQuestionActiveValidator : AbstractValidator<SetQuestionActiveCommand>
{
    public SetQuestionActiveValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}