using OnlineTesting.Domain.Common;
namespace OnlineTesting.Domain.Tests;


public class Bilet : Entity
{
    private readonly List<BiletQuestion> _biletQuestions = new();

    public int Number { get; private set; }
    public bool IsDemo { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    public IReadOnlyCollection<BiletQuestion> BiletQuestions => _biletQuestions.AsReadOnly();

    public const int MinQuestionsCount = 1;

    private Bilet() { }

    public static Bilet Create(int number, IReadOnlyList<Guid> questionIds, bool isDemo)
    {
        ValidateNumber(number);
        ValidateQuestionIds(questionIds);

        var now = DateTime.UtcNow;
        var bilet = new Bilet
        {
            Id = Guid.NewGuid(),
            Number = number,
            IsDemo = isDemo,
            IsActive = true,
            CreatedAt = now,
            UpdatedAt = now
        };

        for (var i = 0; i < questionIds.Count; i++)
        {
            bilet._biletQuestions.Add(new BiletQuestion
            {
                BiletId = bilet.Id,
                QuestionId = questionIds[i],
                OrderIndex = i + 1
            });
        }

        return bilet;
    }

    public void ReplaceQuestions(IReadOnlyList<Guid> questionIds)
    {
        ValidateQuestionIds(questionIds);

        _biletQuestions.Clear();
        for (var i = 0; i < questionIds.Count; i++)
        {
            _biletQuestions.Add(new BiletQuestion
            {
                BiletId = Id,
                QuestionId = questionIds[i],
                OrderIndex = i + 1
            });
        }
        Touch();
    }

    public void MarkAsDemo()
    {
        if (IsDemo) return;
        IsDemo = true;
        Touch();
    }

    public void UnmarkAsDemo()
    {
        if (!IsDemo) return;
        IsDemo = false;
        Touch();
    }

    public void Activate()
    {
        if (IsActive) return;
        IsActive = true;
        Touch();
    }

    public void Deactivate()
    {
        if (!IsActive) return;
        IsActive = false;
        Touch();
    }

    private void Touch() => UpdatedAt = DateTime.UtcNow;

    private static void ValidateNumber(int number)
    {
        if (number <= 0)
            throw new ArgumentException("Bilet number must be positive.", nameof(number));
    }

    private static void ValidateQuestionIds(IReadOnlyList<Guid> questionIds)
    {
        if (questionIds is null)
            throw new ArgumentNullException(nameof(questionIds));

        if (questionIds.Count < MinQuestionsCount)
            throw new ArgumentException(
                $"Bilet must contain at least {MinQuestionsCount} question(s), got {questionIds.Count}.",
                nameof(questionIds));

        if (questionIds.Any(id => id == Guid.Empty))
            throw new ArgumentException("Question id cannot be empty.", nameof(questionIds));

        if (questionIds.Distinct().Count() != questionIds.Count)
            throw new ArgumentException("Duplicate question ids within a single bilet are not allowed.", nameof(questionIds));
    }
}