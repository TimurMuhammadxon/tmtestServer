using MediatR;
using Microsoft.EntityFrameworkCore;
using OnlineTesting.Application.Common.Exceptions;
using OnlineTesting.Application.Common.Interfaces;

namespace OnlineTesting.Application.Tests.Admin.Bilets.Commands.UpdateBilet;

public class UpdateBiletHandler : IRequestHandler<UpdateBiletCommand>
{
    private readonly IApplicationDbContext _db;
    private readonly IDbExceptionInspector _dbInspector;

    public UpdateBiletHandler(IApplicationDbContext db, IDbExceptionInspector dbInspector)
    {
        _db = db;
        _dbInspector = dbInspector;
    }

    public async Task Handle(UpdateBiletCommand request, CancellationToken ct)
    {
        var bilet = await _db.Bilets
            .Include(b => b.BiletQuestions)
            .FirstOrDefaultAsync(b => b.Id == request.Id, ct)
            ?? throw new NotFoundException($"Bilet '{request.Id}' not found.");

        // 1. Все QuestionIds существуют
        var existingIds = await _db.Questions
            .Where(q => request.QuestionIds.Contains(q.Id))
            .Select(q => q.Id)
            .ToListAsync(ct);

        var missing = request.QuestionIds.Except(existingIds).ToList();
        if (missing.Count > 0)
            throw new NotFoundException(
                $"Questions not found: {string.Join(", ", missing)}");

        // 2. Вопросы не в других билетах (кроме текущего)
        await EnsureQuestionsNotUsedAsync(request.QuestionIds, request.Id, ct);

        // 3. Транзакционный replace: сначала удалить старые, потом вставить новые.
        // Это нужно потому что ux_bilet_questions_question (unique) не позволяет
        // временно держать тот же question_id в старой и новой строке одновременно.
        await using var tx = await _db.BeginTransactionAsync(ct);

        try
        {
            // Удаляем все текущие BiletQuestions через DbSet (не через aggregate),
            // чтобы EF точно сделал DELETE до INSERT.
            var oldBiletQuestions = await _db.BiletQuestions
                .Where(bq => bq.BiletId == request.Id)
                .ToListAsync(ct);

            _db.BiletQuestions.RemoveRange(oldBiletQuestions);
            await _db.SaveChangesAsync(ct);

            // Обновляем aggregate в памяти и сохраняем новые BiletQuestions
            bilet.ReplaceQuestions(request.QuestionIds);
            await _db.SaveChangesAsync(ct);

            await tx.CommitAsync(ct);
        }
        catch (DbUpdateException ex) when (_dbInspector.IsUniqueConstraintViolation(ex))
        {
            await tx.RollbackAsync(ct);

            // Race condition: между проверкой и вставкой кто-то занял вопрос.
            await EnsureQuestionsNotUsedAsync(request.QuestionIds, request.Id, ct);
            throw new ConflictException("Concurrent modification detected. Please retry.");
        }
    }

    private async Task EnsureQuestionsNotUsedAsync(
        IReadOnlyList<Guid> questionIds,
        Guid excludeBiletId,
        CancellationToken ct)
    {
        var alreadyUsed = await _db.BiletQuestions
            .Where(bq => questionIds.Contains(bq.QuestionId) && bq.BiletId != excludeBiletId)
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
}