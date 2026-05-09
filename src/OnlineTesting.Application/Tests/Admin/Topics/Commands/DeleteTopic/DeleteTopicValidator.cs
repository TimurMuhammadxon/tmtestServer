using FluentValidation;

namespace OnlineTesting.Application.Tests.Admin.Topics.Commands.DeleteTopic;

public class DeleteTopicValidator : AbstractValidator<DeleteTopicCommand>
{
    public DeleteTopicValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}