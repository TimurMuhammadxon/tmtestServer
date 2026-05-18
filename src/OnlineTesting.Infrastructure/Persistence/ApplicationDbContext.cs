using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using OnlineTesting.Application.Common.Interfaces;
using OnlineTesting.Domain.Progress;
using OnlineTesting.Domain.Tests;
using OnlineTesting.Domain.Users;

namespace OnlineTesting.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<ExternalLogin> ExternalLogins => Set<ExternalLogin>();

    public DbSet<Language> Languages => Set<Language>();
    public DbSet<Topic> Topics => Set<Topic>();
    public DbSet<TopicTranslation> TopicTranslations => Set<TopicTranslation>();
    public DbSet<Question> Questions => Set<Question>();
    public DbSet<QuestionTranslation> QuestionTranslations => Set<QuestionTranslation>();
    public DbSet<Answer> Answers => Set<Answer>();
    public DbSet<AnswerTranslation> AnswerTranslations => Set<AnswerTranslation>();
    public DbSet<Bilet> Bilets => Set<Bilet>();
    public DbSet<BiletQuestion> BiletQuestions => Set<BiletQuestion>();
    public DbSet<Attempt> Attempts => Set<Attempt>();
    public DbSet<AttemptQuestion> AttemptQuestions => Set<AttemptQuestion>();
    public DbSet<UserDailyActivity> UserDailyActivities => Set<UserDailyActivity>();

    public Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
        => Database.BeginTransactionAsync(cancellationToken);

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}