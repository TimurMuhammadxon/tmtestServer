using MediatR;
using Microsoft.EntityFrameworkCore;
using OnlineTesting.Application.Common.Exceptions;
using OnlineTesting.Application.Common.Interfaces;

namespace OnlineTesting.Application.Tests.Admin.Questions.Commands.UploadQuestionImage;

public class UploadQuestionImageHandler : IRequestHandler<UploadQuestionImageCommand, UploadQuestionImageResult>
{
    private readonly IApplicationDbContext _db;
    private readonly IStorageService _storage;

    public UploadQuestionImageHandler(IApplicationDbContext db, IStorageService storage)
    {
        _db = db;
        _storage = storage;
    }

    public async Task<UploadQuestionImageResult> Handle(UploadQuestionImageCommand request, CancellationToken ct)
    {
        var question = await _db.Questions.FirstOrDefaultAsync(q => q.Id == request.QuestionId, ct)
            ?? throw new NotFoundException($"Question '{request.QuestionId}' not found.");

        var ext = request.ContentType switch
        {
            "image/jpeg" => ".jpg",
            "image/png"  => ".png",
            "image/webp" => ".webp",
            "image/gif"  => ".gif",
            _ => throw new ValidationException([new("contentType", $"Unsupported image type: {request.ContentType}. Allowed: jpeg, png, webp, gif.")])
        };

        if (question.ImageKey is not null)
            await _storage.DeleteAsync(question.ImageKey, ct);

        var key = $"questions/{question.Id}{ext}";
        await _storage.UploadAsync(key, request.Content, request.ContentType, ct);

        question.SetImage(key);
        await _db.SaveChangesAsync(ct);

        return new UploadQuestionImageResult(key, _storage.GetPublicUrl(key));
    }
}
