using MediatR;
using Microsoft.EntityFrameworkCore;
using OnlineTesting.Application.Common.Exceptions;
using OnlineTesting.Application.Common.Interfaces;

namespace OnlineTesting.Application.Tests.Admin.Bilets.Commands.DeactivateBilet;

public class DeactivateBiletHandler : IRequestHandler<DeactivateBiletCommand>
{
    private readonly IApplicationDbContext _db;
    public DeactivateBiletHandler(IApplicationDbContext db) => _db = db;

    public async Task Handle(DeactivateBiletCommand request, CancellationToken ct)
    {
        var bilet = await _db.Bilets
            .FirstOrDefaultAsync(b => b.Id == request.Id, ct)
            ?? throw new NotFoundException($"Bilet '{request.Id}' not found.");

        bilet.Deactivate();
        await _db.SaveChangesAsync(ct);
    }
}