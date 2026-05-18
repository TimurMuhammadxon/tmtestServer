namespace OnlineTesting.Domain.Teacher;

public class GroupMember
{
    public Guid GroupId { get; private set; }
    public Guid UserId { get; private set; }
    public DateTime JoinedAt { get; private set; }

    private GroupMember() { }

    public static GroupMember Create(Guid groupId, Guid userId) => new()
    {
        GroupId = groupId,
        UserId = userId,
        JoinedAt = DateTime.UtcNow
    };
}
