using FluentValidation;

namespace OnlineTesting.Application.Tests.Solutions.Attempts.Commands.SubmitAnswer;

public class SubmitAnswerValidator : AbstractValidator<SubmitAnswerCommand>
{
    public SubmitAnswerValidator()
    {
        RuleFor(x => x.AttemptId).NotEqual(Guid.Empty);
        RuleFor(x => x.QuestionId).NotEqual(Guid.Empty);
        RuleFor(x => x.AnswerId).NotEqual(Guid.Empty);
    }
}
