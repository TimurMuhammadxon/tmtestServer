using System.Security.Cryptography;
using OnlineTesting.Domain.Common;
using OnlineTesting.Domain.Tests;

namespace OnlineTesting.Domain.Teacher;

public class TestLink : Entity
{
    private static readonly char[] LinkChars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789".ToCharArray();

    public Guid TeacherId { get; private set; }
    public string Title { get; private set; } = default!;
    public string Code { get; private set; } = default!;
    public FlowType FlowType { get; private set; }
    public Guid? BiletId { get; private set; }
    public List<Guid> TopicIds { get; private set; } = new();
    public int? QuestionCount { get; private set; }
    public Guid? GroupId { get; private set; }
    public int MaxAttempts { get; private set; }
    public DateTime ExpiresAt { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private TestLink() { }

    public static TestLink Create(
        Guid teacherId,
        string title,
        FlowType flowType,
        Guid? biletId,
        List<Guid>? topicIds,
        int? questionCount,
        Guid? groupId,
        int maxAttempts,
        DateTime expiresAt)
    {
        return new TestLink
        {
            Id = Guid.NewGuid(),
            TeacherId = teacherId,
            Title = title.Trim(),
            Code = GenerateCode(),
            FlowType = flowType,
            BiletId = biletId,
            TopicIds = topicIds ?? new List<Guid>(),
            QuestionCount = questionCount,
            GroupId = groupId,
            MaxAttempts = maxAttempts,
            ExpiresAt = expiresAt,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Activate() => IsActive = true;
    public void Deactivate() => IsActive = false;

    public void Update(string title, int maxAttempts, DateTime expiresAt)
    {
        Title = title.Trim();
        MaxAttempts = maxAttempts;
        ExpiresAt = expiresAt;
    }

    private static string GenerateCode()
    {
        var bytes = new byte[8];
        RandomNumberGenerator.Fill(bytes);
        return new string(bytes.Select(b => LinkChars[b % LinkChars.Length]).ToArray());
    }
}
