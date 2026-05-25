using MediatR;
using Microsoft.EntityFrameworkCore;
using OnlineTesting.Application.Common.Interfaces;
using OnlineTesting.Application.Tests.Solutions.Bilets.Queries;

namespace OnlineTesting.Application.Tests.Solutions.Bilets.Queries.GetPublicBilets;

public class GetPublicBiletsHandler
    : IRequestHandler<GetPublicBiletsQuery, IReadOnlyList<PublicBiletListItemDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUser _currentUser;

    public GetPublicBiletsHandler(IApplicationDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyList<PublicBiletListItemDto>> Handle(
        GetPublicBiletsQuery request, CancellationToken ct)
    {
        var query = _db.Bilets.AsNoTracking().Where(b => b.IsActive);

        if (!_currentUser.IsAuthenticated)
            query = query.Where(b => b.IsDemo);

        return await query
            .OrderBy(b => b.Number)
            .Select(b => new PublicBiletListItemDto(b.Id, b.Number, b.IsDemo, b.BiletQuestions.Count))
            .ToListAsync(ct);
    }
}