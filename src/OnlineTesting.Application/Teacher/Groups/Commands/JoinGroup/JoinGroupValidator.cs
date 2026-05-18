using FluentValidation;

namespace OnlineTesting.Application.Teacher.Groups.Commands.JoinGroup;

public class JoinGroupValidator : AbstractValidator<JoinGroupCommand>
{
    public JoinGroupValidator()
    {
        RuleFor(x => x.InviteCode).NotEmpty().MaximumLength(10);
    }
}
