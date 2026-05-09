using FluentValidation;

namespace OnlineTesting.Application.Tests.Admin.Questions.Commands.DeleteQuestion;

public class DeleteQuestionValidator : AbstractValidator<DeleteQuestionCommand>
{
    public DeleteQuestionValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}