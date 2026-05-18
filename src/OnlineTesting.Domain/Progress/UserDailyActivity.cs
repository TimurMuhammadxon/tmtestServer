namespace OnlineTesting.Domain.Progress;

public class UserDailyActivity
{
    public Guid UserId { get; private set; }
    public DateOnly ActivityDate { get; private set; }

    private UserDailyActivity() { }

    public static UserDailyActivity Create(Guid userId, DateOnly date) => new()
    {
        UserId = userId,
        ActivityDate = date
    };
}
