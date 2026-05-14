using MediatR;
using Microsoft.EntityFrameworkCore;
using OnlineTesting.Application.Common.Exceptions;
using OnlineTesting.Application.Common.Interfaces;
using OnlineTesting.Domain.Tests;

namespace OnlineTesting.Application.Tests.Admin.Bilets.Commands.CreateBilet;

public class CreateBiletHandler : IRequestHandler<CreateBiletCommand, Guid>
{
    private readonly IApplicationDbContext _db;
    private readonly IDbExceptionInspector _dbInspector;

    public CreateBiletHandler(IApplicationDbContext db, IDbExceptionInspector dbInspector)
    {
        _db = db;
        _dbInspector = dbInspector;
    }

    public async Task<Guid> Handle(CreateBiletCommand request, CancellationToken ct)
    {
        // 1. Уникальность Number
        var numberTaken = await _db.Bilets
            .AnyAsync(b => b.Number == request.Number, ct);
        if (numberTaken)
            throw new ConflictException($"Bilet with number {request.Number} already exists.");

        // 2. Все QuestionIds существуют
        var existingIds = await _db.Questions
            .Where(q => request.QuestionIds.Contains(q.Id))
            .Select(q => q.Id)
            .ToListAsync(ct);

        var missing = request.QuestionIds.Except(existingIds).ToList();
        if (missing.Count > 0)
            throw new NotFoundException(
                $"Questions not found: {string.Join(", ", missing)}");

        // 3. Вопросы не используются в других билетах
        await EnsureQuestionsNotUsedAsync(request.QuestionIds, excludeBiletId: null, ct);

        // 4. Демо-флаг — один на всю систему
        if (request.IsDemo)
        {
            var demoExists = await _db.Bilets.AnyAsync(b => b.IsDemo, ct);
            if (demoExists)
                throw new ConflictException("A demo bilet already exists.");
        }

        // 5. Создание
        var bilet = Bilet.Create(request.Number, request.QuestionIds, request.IsDemo);
        _db.Bilets.Add(bilet);

        try
        {
            await _db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex) when (_dbInspector.IsUniqueConstraintViolation(ex))
        {
            // Race condition: между pre-check и SaveChanges кто-то занял Number, вопрос или demo.
            // Делаем повторную проверку для понятного сообщения.
            await ThrowDetailedConflictAsync(request, excludeBiletId: null, ct);
            throw new ConflictException("Concurrent modification detected. Please retry.");
        }

        return bilet.Id;
    }

    private async Task EnsureQuestionsNotUsedAsync(
        IReadOnlyList<Guid> questionIds,
        Guid? excludeBiletId,
        CancellationToken ct)
    {
        var alreadyUsed = await _db.BiletQuestions
            .Where(bq => questionIds.Contains(bq.QuestionId)
                         && (excludeBiletId == null || bq.BiletId != excludeBiletId))
            .Select(bq => new { bq.QuestionId, bq.Bilet!.Number })
            .ToListAsync(ct);

        if (alreadyUsed.Count > 0)
        {
            var details = string.Join(", ",
                alreadyUsed.Select(x => $"{x.QuestionId} (in bilet #{x.Number})"));
            throw new ConflictException(
                $"Some questions are already used in other bilets: {details}");
        }
    }

    private async Task ThrowDetailedConflictAsync(
        CreateBiletCommand request,
        Guid? excludeBiletId,
        CancellationToken ct)
    {
        // Number
        var numberTaken = await _db.Bilets.AnyAsync(b => b.Number == request.Number, ct);
        if (numberTaken)
            throw new ConflictException($"Bilet with number {request.Number} already exists.");

        // Demo
        if (request.IsDemo && await _db.Bilets.AnyAsync(b => b.IsDemo, ct))
            throw new ConflictException("A demo bilet already exists.");

        // Questions
        await EnsureQuestionsNotUsedAsync(request.QuestionIds, excludeBiletId, ct);
    }
}