using MediatR;
using Microsoft.EntityFrameworkCore;
using OnlineTesting.Application.Common.Exceptions;
using OnlineTesting.Application.Common.Interfaces;

namespace OnlineTesting.Application.Tests.Admin.Bilets.Commands.MarkBiletAsDemo;

public class MarkBiletAsDemoHandler : IRequestHandler<MarkBiletAsDemoCommand>
{
    private readonly IApplicationDbContext _db;
    private readonly IDbExceptionInspector _dbInspector;

    public MarkBiletAsDemoHandler(IApplicationDbContext db, IDbExceptionInspector dbInspector)
    {
        _db = db;
        _dbInspector = dbInspector;
    }

    public async Task Handle(MarkBiletAsDemoCommand request, CancellationToken ct)
    {
        var bilet = await _db.Bilets
            .FirstOrDefaultAsync(b => b.Id == request.Id, ct)
            ?? throw new NotFoundException($"Bilet '{request.Id}' not found.");

        if (bilet.IsDemo) return;

        var existingDemo = await _db.Bilets
            .Where(b => b.IsDemo && b.Id != request.Id)
            .Select(b => (int?)b.Number)
            .FirstOrDefaultAsync(ct);

        if (existingDemo.HasValue)
            throw new ConflictException($"A demo bilet already exists (bilet #{existingDemo.Value}).");

        bilet.MarkAsDemo();

        try
        {
            await _db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex) when (_dbInspector.IsUniqueConstraintViolation(ex))
        {
            // Race: кто-то пометил другой билет демо между проверкой и SaveChanges.
            var racedDemo = await _db.Bilets
                .Where(b => b.IsDemo && b.Id != request.Id)
                .Select(b => (int?)b.Number)
                .FirstOrDefaultAsync(ct);

            throw new ConflictException(racedDemo.HasValue
                ? $"A demo bilet already exists (bilet #{racedDemo.Value})."
                : "A demo bilet already exists.");
        }
    }
}