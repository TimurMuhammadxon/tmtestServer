using FluentValidation;

namespace OnlineTesting.Application.Teacher.Applications.Commands.SubmitApplication;

public class SubmitTeacherApplicationValidator : AbstractValidator<SubmitTeacherApplicationCommand>
{
    public SubmitTeacherApplicationValidator()
    {
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.PhoneNumber).NotEmpty().MaximumLength(30);
        RuleFor(x => x.TelegramUsername).MaximumLength(100).When(x => x.TelegramUsername is not null);
        RuleFor(x => x.OrganizationName).MaximumLength(200).When(x => x.OrganizationName is not null);
        RuleFor(x => x.ExperienceText).MaximumLength(2000).When(x => x.ExperienceText is not null);
        RuleFor(x => x.AdditionalNotes).MaximumLength(1000).When(x => x.AdditionalNotes is not null);
    }
}
