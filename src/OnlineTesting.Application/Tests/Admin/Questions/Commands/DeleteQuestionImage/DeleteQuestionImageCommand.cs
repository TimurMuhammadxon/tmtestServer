using MediatR;

namespace OnlineTesting.Application.Tests.Admin.Questions.Commands.DeleteQuestionImage;

public record DeleteQuestionImageCommand(Guid QuestionId) : IRequest;
