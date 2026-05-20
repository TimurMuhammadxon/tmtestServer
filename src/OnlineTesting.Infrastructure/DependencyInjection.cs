using System.Text;
using Amazon.S3;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using OnlineTesting.Application.Common.Interfaces;
using OnlineTesting.Infrastructure.Authentication;
using OnlineTesting.Infrastructure.Persistence;
using OnlineTesting.Infrastructure.Storage;
using OnlineTesting.Infrastructure.Subscriptions;

namespace OnlineTesting.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        AddPersistence(services, configuration);
        AddAuthentication(services, configuration);
        AddTelegram(services, configuration);
        AddStorage(services, configuration);

        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IRefreshTokenService, RefreshTokenService>();
        services.AddScoped<IJwtService, JwtService>();
        services.AddSingleton<IDbExceptionInspector, PostgresExceptionInspector>();
        services.AddScoped<ISubscriptionChecker, SubscriptionChecker>();

        return services;
    }

    private static void AddPersistence(IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");

        services.AddDbContext<ApplicationDbContext>(options =>
            options
                .UseNpgsql(connectionString)
                .UseSnakeCaseNamingConvention());

        services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<ApplicationDbContext>());
    }

    private static void AddAuthentication(IServiceCollection services, IConfiguration configuration)
    {
        var jwtSection = configuration.GetSection(JwtOptions.SectionName);
        services.Configure<JwtOptions>(jwtSection);

        var jwtOptions = jwtSection.Get<JwtOptions>()
            ?? throw new InvalidOperationException("Jwt section is not configured.");

        if (string.IsNullOrWhiteSpace(jwtOptions.Key) || jwtOptions.Key.Length < 32)
            throw new InvalidOperationException("Jwt:Key must be configured and at least 32 characters long.");
        if (string.IsNullOrWhiteSpace(jwtOptions.Issuer))
            throw new InvalidOperationException("Jwt:Issuer must be configured.");
        if (string.IsNullOrWhiteSpace(jwtOptions.Audience))
            throw new InvalidOperationException("Jwt:Audience must be configured.");

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.MapInboundClaims = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ClockSkew = TimeSpan.FromSeconds(30),
                    ValidIssuer = jwtOptions.Issuer,
                    ValidAudience = jwtOptions.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Key)),
                    NameClaimType = "sub",
                    RoleClaimType = System.Security.Claims.ClaimTypes.Role
                };

                // Для endpoints с [AllowAnonymous]: битый/истёкший токен не валит запрос.
                // [Authorize] endpoints всё равно отклонят запрос на этапе авторизации.
                options.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = context =>
                    {
                        context.NoResult();
                        return Task.CompletedTask;
                    }
                };
            });
    }

    private static void AddStorage(IServiceCollection services, IConfiguration configuration)
    {
        var section = configuration.GetSection(StorageOptions.SectionName);
        services.Configure<StorageOptions>(section);

        var opts = section.Get<StorageOptions>()
            ?? throw new InvalidOperationException("Storage section is not configured.");

        var s3Config = new AmazonS3Config
        {
            ServiceURL = $"{(opts.UseHttps ? "https" : "http")}://{opts.Endpoint}",
            ForcePathStyle = true,
            AuthenticationRegion = "us-east-1"
        };

        services.AddSingleton<IAmazonS3>(_ =>
            new AmazonS3Client(opts.AccessKey, opts.SecretKey, s3Config));

        services.AddSingleton<IStorageService, MinioStorageService>();
        services.AddHostedService<BucketInitializer>();
    }

    private static void AddTelegram(IServiceCollection services, IConfiguration configuration)
    {
        var section = configuration.GetSection(TelegramOptions.SectionName);
        services.Configure<TelegramOptions>(section);

        var options = section.Get<TelegramOptions>()
            ?? throw new InvalidOperationException("Telegram section is not configured.");

        if (string.IsNullOrWhiteSpace(options.BotToken))
            throw new InvalidOperationException("Telegram:BotToken must be configured.");

        if (options.AuthDateExpirationHours <= 0)
            throw new InvalidOperationException("Telegram:AuthDateExpirationHours must be positive.");

        // Singleton: валидатор без mutable state, secret_key кэшируется в конструкторе.
        services.AddSingleton<ITelegramAuthValidator, TelegramAuthValidator>();
    }
}