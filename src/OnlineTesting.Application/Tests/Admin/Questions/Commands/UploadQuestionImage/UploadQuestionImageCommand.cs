using MediatR;

namespace OnlineTesting.Application.Tests.Admin.Questions.Commands.UploadQuestionImage;

public record UploadQuestionImageCommand(
    Guid QuestionId,
    Stream Content,
    string ContentType
) : IRequest<UploadQuestionImageResult>;

public record UploadQuestionImageResult(string ImageKey, string Url);
