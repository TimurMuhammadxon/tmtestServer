using MediatR;
using Microsoft.EntityFrameworkCore;
using OnlineTesting.Application.Common.Exceptions;
using OnlineTesting.Application.Common.Interfaces;
using OnlineTesting.Domain.Content;

namespace OnlineTesting.Application.Content.RoadSigns.Commands;

public record SignFullTranslationInput(string Language, string Name, string? Description);

// ── Create sign ────────────────────────────────────────────────────
public record CreateRoadSignCommand(
    Guid CategoryId,
    string? Code,
    int OrderIndex,
    List<SignFullTranslationInput> Translations) : IRequest<Guid>;

public class CreateRoadSignHandler : IRequestHandler<CreateRoadSignCommand, Guid>
{
    private readonly IApplicationDbContext _db;
    public CreateRoadSignHandler(IApplicationDbContext db) => _db = db;

    public async Task<Guid> Handle(CreateRoadSignCommand request, CancellationToken ct)
    {
        if (request.Translations is null || request.Translations.Count == 0)
            throw new ValidationException(new[] { new FluentValidation.Results.ValidationFailure("translations", "At least one translation is required.") });

        var categoryExists = await _db.RoadSignCategories.AnyAsync(c => c.Id == request.CategoryId, ct);
        if (!categoryExists)
            throw new NotFoundException($"Category '{request.CategoryId}' not found.");

        var sign = RoadSign.Create(
            request.CategoryId,
            request.Code,
            request.OrderIndex,
            request.Translations.Select(t => (t.Language, t.Name, t.Description)));

        _db.RoadSigns.Add(sign);
        await _db.SaveChangesAsync(ct);
        return sign.Id;
    }
}

// ── Update sign ────────────────────────────────────────────────────
public record UpdateRoadSignCommand(
    Guid Id,
    Guid CategoryId,
    string? Code,
    int OrderIndex,
    bool IsActive,
    List<SignFullTranslationInput> Translations) : IRequest;

public class UpdateRoadSignHandler : IRequestHandler<UpdateRoadSignCommand>
{
    private readonly IApplicationDbContext _db;
    public UpdateRoadSignHandler(IApplicationDbContext db) => _db = db;

    public async Task Handle(UpdateRoadSignCommand request, CancellationToken ct)
    {
        var sign = await _db.RoadSigns
            .Include(s => s.Translations)
            .FirstOrDefaultAsync(s => s.Id == request.Id, ct)
            ?? throw new NotFoundException($"Sign '{request.Id}' not found.");

        var categoryExists = await _db.RoadSignCategories.AnyAsync(c => c.Id == request.CategoryId, ct);
        if (!categoryExists)
            throw new NotFoundException($"Category '{request.CategoryId}' not found.");

        sign.UpdateBasics(request.CategoryId, request.Code, request.OrderIndex);
        if (request.IsActive) sign.Activate(); else sign.Deactivate();
        foreach (var t in request.Translations ?? new())
            sign.UpsertTranslation(t.Language, t.Name, t.Description);

        await _db.SaveChangesAsync(ct);
    }
}

// ── Delete sign ────────────────────────────────────────────────────
public record DeleteRoadSignCommand(Guid Id) : IRequest;

public class DeleteRoadSignHandler : IRequestHandler<DeleteRoadSignCommand>
{
    private readonly IApplicationDbContext _db;
    private readonly IStorageService _storage;
    public DeleteRoadSignHandler(IApplicationDbContext db, IStorageService storage)
    {
        _db = db;
        _storage = storage;
    }

    public async Task Handle(DeleteRoadSignCommand request, CancellationToken ct)
    {
        var sign = await _db.RoadSigns.FirstOrDefaultAsync(s => s.Id == request.Id, ct)
            ?? throw new NotFoundException($"Sign '{request.Id}' not found.");

        if (!string.IsNullOrWhiteSpace(sign.ImageKey))
            await _storage.DeleteAsync(sign.ImageKey!, ct);

        _db.RoadSigns.Remove(sign);
        await _db.SaveChangesAsync(ct);
    }
}
