namespace OnlineTesting.Domain.Tests;

public class AttemptQuestion
{
    public Guid AttemptId { get; private set; }
    public Guid QuestionId { get; private set; }
    public int OrderIndex { get; private set; }
    public Guid? ChosenAnswerId { get; private set; }
    public bool? IsCorrect { get; private set; }
    public DateTime? AnsweredAt { get; private set; }

    public Attempt? Attempt { get; private set; }
    public Question? Question { get; private set; }

    private AttemptQuestion() { }

    internal static AttemptQuestion Create(Guid attemptId, Guid questionId, int orderIndex)
        => new() { AttemptId = attemptId, QuestionId = questionId, OrderIndex = orderIndex };

    internal void SetAnswer(Guid answerId, bool isCorrect)
    {
        ChosenAnswerId = answerId;
        IsCorrect = isCorrect;
        AnsweredAt = DateTime.UtcNow;
    }
}
