using Microsoft.EntityFrameworkCore;
using OnlineTesting.Domain.Users;
using OnlineTesting.Domain.Tests; 

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

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}