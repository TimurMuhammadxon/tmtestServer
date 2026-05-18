using OnlineTesting.Domain.Common;

namespace OnlineTesting.Domain.Tests;

public class Attempt : Entity
{
    private readonly List<AttemptQuestion> _questions = new();

    public Guid UserId { get; private set; }
    public FlowType Flow { get; private set; }
    public AttemptStatus Status { get; private set; }
    public DateTime StartedAt { get; private set; }
    public DateTime? FinishedAt { get; private set; }
    public int? CorrectCount { get; private set; }
    public Guid? BiletId { get; private set; }

    public IReadOnlyCollection<AttemptQuestion> Questions => _questions.AsReadOnly();

    public const int ExamQuestionsCount = 20;
    public const int ExamTimeLimitSeconds = 1500;
    public const int ExamMaxMistakes = 2;
    public const int ExamPassThreshold = 18;

    private Attempt() { }

    public static Attempt Start(Guid userId, FlowType flowType, IReadOnlyList<Guid> questionIds, Guid? biletId = null)
    {
        var attempt = new Attempt
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Flow = flowType,
            Status = AttemptStatus.InProgress,
            StartedAt = DateTime.UtcNow,
            BiletId = biletId
        };

        for (var i = 0; i < questionIds.Count; i++)
            attempt._questions.Add(AttemptQuestion.Create(attempt.Id, questionIds[i], i + 1));

        return attempt;
    }

    // Returns true if the attempt was auto-finished (exam: 3rd mistake)
    public bool Answer(Guid questionId, Guid answerId, bool isCorrect)
    {
        var aq = _questions.FirstOrDefault(q => q.QuestionId == questionId)
            ?? throw new InvalidOperationException($"Question {questionId} is not part of this attempt.");

        aq.SetAnswer(answerId, isCorrect);

        if (Flow == FlowType.Exam)
        {
            var mistakeCount = _questions.Count(q => q.IsCorrect == false);
            if (mistakeCount > ExamMaxMistakes)
            {
                Finish();
                return true;
            }
        }

        return false;
    }

    public void Finish()
    {
        if (Status != AttemptStatus.InProgress)
            return;

        FinishedAt = DateTime.UtcNow;
        var correct = _questions.Count(q => q.IsCorrect == true);
        CorrectCount = correct;

        Status = Flow == FlowType.Exam
            ? (correct >= ExamPassThreshold ? AttemptStatus.Passed : AttemptStatus.Failed)
            : AttemptStatus.Completed;
    }
}
