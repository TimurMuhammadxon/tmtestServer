using MediatR;
using Microsoft.EntityFrameworkCore;
using OnlineTesting.Application.Common.Exceptions;
using OnlineTesting.Application.Common.Interfaces;

namespace OnlineTesting.Application.Tests.Admin.Topics.Commands.UpdateTopic;

public class UpdateTopicHandler : IRequestHandler<UpdateTopicCommand>
{
    private readonly IApplicationDbContext _db;
    private readonly IDbExceptionInspector _inspector;

    public UpdateTopicHandler(IApplicationDbContext db, IDbExceptionInspector inspector)
    {
        _db = db;
        _inspector = inspector;
    }

    public async Task Handle(UpdateTopicCommand request, CancellationToken ct)
    {
        var topic = await _db.Topics.FirstOrDefaultAsync(t => t.Id == request.Id, ct)
            ?? throw new NotFoundException($"Topic '{request.Id}' not found.");

        topic.UpdateBasics(request.Code, request.OrderIndex);
        topic.SetDemo(request.IsDemo);

        try
        {
            await _db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex) when (_inspector.IsUniqueConstraintViolation(ex))
        {
            throw new ConflictException($"Topic with code '{request.Code}' already exists.");
        }
    }
}