using MediatR;
using Microsoft.EntityFrameworkCore;
using OnlineTesting.Application.Common.Exceptions;
using OnlineTesting.Application.Common.Interfaces;

namespace OnlineTesting.Application.Tests.Admin.Bilets.Commands.DeleteBilet;

public class DeleteBiletHandler : IRequestHandler<DeleteBiletCommand>
{
    private readonly IApplicationDbContext _db;
    public DeleteBiletHandler(IApplicationDbContext db) => _db = db;

    public async Task Handle(DeleteBiletCommand request, CancellationToken ct)
    {
        var bilet = await _db.Bilets
            .FirstOrDefaultAsync(b => b.Id == request.Id, ct)
            ?? throw new NotFoundException($"Bilet '{request.Id}' not found.");

        _db.Bilets.Remove(bilet);
        await _db.SaveChangesAsync(ct);
    }
}