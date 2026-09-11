using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.RateLimiting;
using OnlineTesting.API.Authorization;
using OnlineTesting.API.Localization;
using OnlineTesting.API.Middleware;
using OnlineTesting.API.Services;
using OnlineTesting.Application;
using OnlineTesting.Application.Common.Interfaces;
using OnlineTesting.Domain.Authorization;
using Microsoft.EntityFrameworkCore;
using OnlineTesting.Infrastructure;
using OnlineTesting.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUser, HttpContextCurrentUser>();
builder.Services.AddScoped<IRequestContext, HttpRequestContext>();
builder.Services.AddScoped<ILanguageContext, LanguageContext>();

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(Roles.Policies.ContentManagement, p =>
        p.RequireRole(Roles.Owner, Roles.SuperAdmin, Roles.Admin));
    options.AddPolicy(Roles.Policies.TeacherAccess, p =>
        p.RequireRole(Roles.Owner, Roles.SuperAdmin, Roles.Admin, Roles.Teacher));
    options.AddPolicy(Roles.Policies.OwnerAccess, p =>
        p.RequireRole(Roles.Owner));
    options.AddPolicy(Roles.Policies.TeacherSubscriptionAccess, p =>
        p.RequireRole(Roles.Owner, Roles.SuperAdmin, Roles.Admin, Roles.Teacher)
         .AddRequirements(new TeacherSubscriptionRequirement()));
});

builder.Services.AddScoped<IAuthorizationHandler, TeacherSubscriptionHandler>();

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    // Login/Register: 10 attempts per minute per IP
    options.AddPolicy("auth-strict", context =>
        RateLimitPartition.GetFixedWindowLimiter(
            context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 10,
                Window = TimeSpan.FromMinutes(1),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 0,
            }));

    // Refresh/Telegram/Google: 20 attempts per minute per IP
    options.AddPolicy("auth-normal", context =>
        RateLimitPartition.GetFixedWindowLimiter(
            context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 20,
                Window = TimeSpan.FromMinutes(1),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 0,
            }));
});

builder.Services.AddResponseCaching();

// Response compression — big win for large JSON (e.g. Marathon attempt with ~1300 questions).
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<Microsoft.AspNetCore.ResponseCompression.BrotliCompressionProvider>();
    options.Providers.Add<Microsoft.AspNetCore.ResponseCompression.GzipCompressionProvider>();
    options.MimeTypes = new[]
    {
        "text/plain", "text/css", "application/javascript", "text/javascript",
        "text/html", "application/xml", "text/xml",
        "application/json", "application/problem+json", "image/svg+xml"
    };
});
builder.Services.Configure<Microsoft.AspNetCore.ResponseCompression.BrotliCompressionProviderOptions>(
    o => o.Level = System.IO.Compression.CompressionLevel.Fastest);
builder.Services.Configure<Microsoft.AspNetCore.ResponseCompression.GzipCompressionProviderOptions>(
    o => o.Level = System.IO.Compression.CompressionLevel.Fastest);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Введите JWT access token (без префикса 'Bearer ')."
    });

    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();
}

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});
app.UseResponseCompression();
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseRateLimiter();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.UseResponseCaching();
app.UseMiddleware<LanguageMiddleware>();
app.MapControllers();

app.Run();