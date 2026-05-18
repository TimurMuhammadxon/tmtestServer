using OnlineTesting.Domain.Common;

namespace OnlineTesting.Domain.Teacher;

public class TeacherApplication : Entity
{
    public Guid UserId { get; private set; }
    public string FullName { get; private set; } = default!;
    public string PhoneNumber { get; private set; } = default!;
    public string? TelegramUsername { get; private set; }
    public string? OrganizationName { get; private set; }
    public string? ExperienceText { get; private set; }
    public string? AdditionalNotes { get; private set; }
    public TeacherApplicationStatus Status { get; private set; }
    public DateTime SubmittedAt { get; private set; }
    public DateTime? ReviewedAt { get; private set; }
    public Guid? ReviewedBy { get; private set; }
    public string? RejectionReason { get; private set; }

    private TeacherApplication() { }

    public static TeacherApplication Submit(
        Guid userId,
        string fullName,
        string phoneNumber,
        string? telegramUsername,
        string? organizationName,
        string? experienceText,
        string? additionalNotes)
    {
        if (string.IsNullOrWhiteSpace(fullName))
            throw new ArgumentException("Full name is required.", nameof(fullName));
        if (string.IsNullOrWhiteSpace(phoneNumber))
            throw new ArgumentException("Phone number is required.", nameof(phoneNumber));

        return new TeacherApplication
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            FullName = fullName.Trim(),
            PhoneNumber = phoneNumber.Trim(),
            TelegramUsername = string.IsNullOrWhiteSpace(telegramUsername) ? null : telegramUsername.Trim(),
            OrganizationName = string.IsNullOrWhiteSpace(organizationName) ? null : organizationName.Trim(),
            ExperienceText = string.IsNullOrWhiteSpace(experienceText) ? null : experienceText.Trim(),
            AdditionalNotes = string.IsNullOrWhiteSpace(additionalNotes) ? null : additionalNotes.Trim(),
            Status = TeacherApplicationStatus.Pending,
            SubmittedAt = DateTime.UtcNow
        };
    }

    public void Approve(Guid reviewerId)
    {
        Status = TeacherApplicationStatus.Approved;
        ReviewedAt = DateTime.UtcNow;
        ReviewedBy = reviewerId;
        RejectionReason = null;
    }

    public void Reject(Guid reviewerId, string? reason)
    {
        Status = TeacherApplicationStatus.Rejected;
        ReviewedAt = DateTime.UtcNow;
        ReviewedBy = reviewerId;
        RejectionReason = string.IsNullOrWhiteSpace(reason) ? null : reason.Trim();
    }
}
