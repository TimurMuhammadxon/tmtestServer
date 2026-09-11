using MediatR;
using Microsoft.EntityFrameworkCore;
using OnlineTesting.Application.Common.Exceptions;
using OnlineTesting.Application.Common.Interfaces;
using OnlineTesting.Domain.Content;

namespace OnlineTesting.Application.Content.RoadSigns.Commands;

public record SignTranslationInput(string Language, string Name);

// ── Create category ────────────────────────────────────────────────
public record CreateRoadSignCategoryCommand(int OrderIndex, List<SignTranslationInput> Translations) : IRequest<Guid>;

public class CreateRoadSignCategoryHandler : IRequestHandler<CreateRoadSignCategoryCommand, Guid>
{
    private readonly IApplicationDbContext _db;
    public CreateRoadSignCategoryHandler(IApplicationDbContext db) => _db = db;

    public async Task<Guid> Handle(CreateRoadSignCategoryCommand request, CancellationToken ct)
    {
        if (request.Translations is null || request.Translations.Count == 0)
            throw new ValidationException(new[] { new FluentValidation.Results.ValidationFailure("translations", "At least one translation is required.") });

        var category = RoadSignCategory.Create(
            request.OrderIndex,
            request.Translations.Select(t => (t.Language, t.Name)));

        _db.RoadSignCategories.Add(category);
        await _db.SaveChangesAsync(ct);
        return category.Id;
    }
}

// ── Update category ────────────────────────────────────────────────
public record UpdateRoadSignCategoryCommand(Guid Id, int OrderIndex, List<SignTranslationInput> Translations) : IRequest;

public class UpdateRoadSignCategoryHandler : IRequestHandler<UpdateRoadSignCategoryCommand>
{
    private readonly IApplicationDbContext _db;
    public UpdateRoadSignCategoryHandler(IApplicationDbContext db) => _db = db;

    public async Task Handle(UpdateRoadSignCategoryCommand request, CancellationToken ct)
    {
        var category = await _db.RoadSignCategories
            .Include(c => c.Translations)
            .FirstOrDefaultAsync(c => c.Id == request.Id, ct)
            ?? throw new NotFoundException($"Category '{request.Id}' not found.");

        category.SetOrder(request.OrderIndex);
        foreach (var t in request.Translations ?? new())
            category.UpsertTranslation(t.Language, t.Name);

        await _db.SaveChangesAsync(ct);
    }
}

// ── Delete category ────────────────────────────────────────────────
public record DeleteRoadSignCategoryCommand(Guid Id) : IRequest;

public class DeleteRoadSignCategoryHandler : IRequestHandler<DeleteRoadSignCategoryCommand>
{
    private readonly IApplicationDbContext _db;
    public DeleteRoadSignCategoryHandler(IApplicationDbContext db) => _db = db;

    public async Task Handle(DeleteRoadSignCategoryCommand request, CancellationToken ct)
    {
        var category = await _db.RoadSignCategories
            .FirstOrDefaultAsync(c => c.Id == request.Id, ct)
            ?? throw new NotFoundException($"Category '{request.Id}' not found.");

        var hasSigns = await _db.RoadSigns.AnyAsync(s => s.CategoryId == request.Id, ct);
        if (hasSigns)
            throw new ConflictException("Cannot delete a category that still has signs.");

        _db.RoadSignCategories.Remove(category);
        await _db.SaveChangesAsync(ct);
    }
}
