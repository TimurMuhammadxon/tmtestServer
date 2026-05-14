using MediatR;
using Microsoft.EntityFrameworkCore;
using OnlineTesting.Application.Common.Exceptions;
using OnlineTesting.Application.Common.Interfaces;

namespace OnlineTesting.Application.Tests.Admin.Bilets.Commands.UnmarkBiletAsDemo;

public class UnmarkBiletAsDemoHandler : IRequestHandler<UnmarkBiletAsDemoCommand>
{
    private readonly IApplicationDbContext _db;
    public UnmarkBiletAsDemoHandler(IApplicationDbContext db) => _db = db;

    public async Task Handle(UnmarkBiletAsDemoCommand request, CancellationToken ct)
    {
        var bilet = await _db.Bilets
            .FirstOrDefaultAsync(b => b.Id == request.Id, ct)
            ?? throw new NotFoundException($"Bilet '{request.Id}' not found.");

        bilet.UnmarkAsDemo();
        await _db.SaveChangesAsync(ct);
    }
}