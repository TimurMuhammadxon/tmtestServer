using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using OnlineTesting.Domain.Progress;
using OnlineTesting.Domain.Teacher;
using OnlineTesting.Domain.Tests;
using OnlineTesting.Domain.Users;

namespace OnlineTesting.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<User> Users { get; }
    DbSet<RefreshToken> RefreshTokens { get; }
    DbSet<ExternalLogin> ExternalLogins { get; }
    DbSet<Language> Languages { get; }
    DbSet<Topic> Topics { get; }
    DbSet<TopicTranslation> TopicTranslations { get; }
    DbSet<Question> Questions { get; }
    DbSet<QuestionTranslation> QuestionTranslations { get; }
    DbSet<Answer> Answers { get; }
    DbSet<AnswerTranslation> AnswerTranslations { get; }
    DbSet<Bilet> Bilets { get; }
    DbSet<BiletQuestion> BiletQuestions { get; }
    DbSet<Attempt> Attempts { get; }
    DbSet<AttemptQuestion> AttemptQuestions { get; }
    DbSet<UserDailyActivity> UserDailyActivities { get; }
    DbSet<TeacherApplication> TeacherApplications { get; }
    DbSet<Group> Groups { get; }
    DbSet<GroupMember> GroupMembers { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);
}