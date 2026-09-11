using MediatR;
using Microsoft.EntityFrameworkCore;
using OnlineTesting.Application.Common.Exceptions;
using OnlineTesting.Application.Common.Interfaces;

namespace OnlineTesting.Application.Content.RoadSigns.Commands;

// ── Upload sign image ──────────────────────────────────────────────
public record UploadRoadSignImageCommand(Guid SignId, Stream Content, string ContentType) : IRequest<UploadRoadSignImageResult>;
public record UploadRoadSignImageResult(string ImageKey, string ImageUrl);

public class UploadRoadSignImageHandler : IRequestHandler<UploadRoadSignImageCommand, UploadRoadSignImageResult>
{
    private readonly IApplicationDbContext _db;
    private readonly IStorageService _storage;

    public UploadRoadSignImageHandler(IApplicationDbContext db, IStorageService storage)
    {
        _db = db;
        _storage = storage;
    }

    public async Task<UploadRoadSignImageResult> Handle(UploadRoadSignImageCommand request, CancellationToken ct)
    {
        var sign = await _db.RoadSigns.FirstOrDefaultAsync(s => s.Id == request.SignId, ct)
            ?? throw new NotFoundException($"Sign '{request.SignId}' not found.");

        var ext = request.ContentType switch
        {
            "image/jpeg" => ".jpg",
            "image/png"  => ".png",
            "image/webp" => ".webp",
            "image/gif"  => ".gif",
            _ => throw new ValidationException([new("contentType", $"Unsupported image type: {request.ContentType}. Allowed: jpeg, png, webp, gif.")])
        };

        if (!string.IsNullOrWhiteSpace(sign.ImageKey))
            await _storage.DeleteAsync(sign.ImageKey!, ct);

        var key = $"road-signs/{sign.Id}{ext}";
        await _storage.UploadAsync(key, request.Content, request.ContentType, ct);

        sign.SetImageKey(key);
        await _db.SaveChangesAsync(ct);

        return new UploadRoadSignImageResult(key, _storage.GetPublicUrl(key));
    }
}

// ── Delete sign image ──────────────────────────────────────────────
public record DeleteRoadSignImageCommand(Guid SignId) : IRequest;

public class DeleteRoadSignImageHandler : IRequestHandler<DeleteRoadSignImageCommand>
{
    private readonly IApplicationDbContext _db;
    private readonly IStorageService _storage;

    public DeleteRoadSignImageHandler(IApplicationDbContext db, IStorageService storage)
    {
        _db = db;
        _storage = storage;
    }

    public async Task Handle(DeleteRoadSignImageCommand request, CancellationToken ct)
    {
        var sign = await _db.RoadSigns.FirstOrDefaultAsync(s => s.Id == request.SignId, ct)
            ?? throw new NotFoundException($"Sign '{request.SignId}' not found.");

        if (!string.IsNullOrWhiteSpace(sign.ImageKey))
            await _storage.DeleteAsync(sign.ImageKey!, ct);

        sign.SetImageKey(null);
        await _db.SaveChangesAsync(ct);
    }
}
