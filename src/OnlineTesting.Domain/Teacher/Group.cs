using System.Security.Cryptography;
using OnlineTesting.Domain.Common;

namespace OnlineTesting.Domain.Teacher;

public class Group : Entity
{
    private static readonly char[] InviteChars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789".ToCharArray();

    private readonly List<GroupMember> _members = new();

    public Guid TeacherId { get; private set; }
    public string Name { get; private set; } = default!;
    public string InviteCode { get; private set; } = default!;
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public IReadOnlyCollection<GroupMember> Members => _members.AsReadOnly();

    private Group() { }

    public static Group Create(Guid teacherId, string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Group name is required.", nameof(name));

        return new Group
        {
            Id = Guid.NewGuid(),
            TeacherId = teacherId,
            Name = name.Trim(),
            InviteCode = GenerateInviteCode(),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Rename(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Group name is required.", nameof(name));
        Name = name.Trim();
    }

    public void RegenerateInviteCode() => InviteCode = GenerateInviteCode();

    private static string GenerateInviteCode()
    {
        var bytes = new byte[8];
        RandomNumberGenerator.Fill(bytes);
        return new string(bytes.Select(b => InviteChars[b % InviteChars.Length]).ToArray());
    }
}
