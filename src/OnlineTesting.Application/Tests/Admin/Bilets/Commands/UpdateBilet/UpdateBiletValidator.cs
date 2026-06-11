using FluentValidation;
using OnlineTesting.Domain.Tests;

namespace OnlineTesting.Application.Tests.Admin.Bilets.Commands.UpdateBilet;

public class UpdateBiletValidator : AbstractValidator<UpdateBiletCommand>
{
    public UpdateBiletValidator()
    {
        RuleFor(x => x.Id).NotEmpty();

        RuleFor(x => x.QuestionIds)
            .NotNull()
            .Must(ids => ids != null && ids.Count >= Bilet.MinQuestionsCount)
                .WithMessage($"Bilet must contain at least {Bilet.MinQuestionsCount} question(s).")
            .Must(ids => ids == null || ids.All(id => id != Guid.Empty))
                .WithMessage("Question ids cannot be empty.")
            .Must(ids => ids == null || ids.Distinct().Count() == ids.Count)
                .WithMessage("Duplicate question ids within a single bilet are not allowed.");
    }
}