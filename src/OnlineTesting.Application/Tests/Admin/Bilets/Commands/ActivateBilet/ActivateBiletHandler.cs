using MediatR;
using Microsoft.EntityFrameworkCore;
using OnlineTesting.Application.Common.Exceptions;
using OnlineTesting.Application.Common.Interfaces;

namespace OnlineTesting.Application.Tests.Admin.Bilets.Commands.ActivateBilet;

public class ActivateBiletHandler : IRequestHandler<ActivateBiletCommand>
{
    private readonly IApplicationDbContext _db;
    public ActivateBiletHandler(IApplicationDbContext db) => _db = db;

    public async Task Handle(ActivateBiletCommand request, CancellationToken ct)
    {
        var bilet = await _db.Bilets
            .FirstOrDefaultAsync(b => b.Id == request.Id, ct)
            ?? throw new NotFoundException($"Bilet '{request.Id}' not found.");

        bilet.Activate();
        await _db.SaveChangesAsync(ct);
    }
}