using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using OnlineTesting.Application.Common.Exceptions;

namespace OnlineTesting.API.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly IHostEnvironment _env;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger,
        IHostEnvironment env)
    {
        _next = next;
        _logger = logger;
        _env = env;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleAsync(context, ex);
        }
    }

    private async Task HandleAsync(HttpContext context, Exception ex)
    {
        ProblemDetails problem;
        int status;

        switch (ex)
        {
            case ValidationException ve:
                status = (int)HttpStatusCode.BadRequest;
                problem = new ValidationProblemDetails(ve.Errors)
                {
                    Status = status,
                    Title = "Validation failed.",
                    Instance = context.Request.Path
                };
                break;

            case ConflictException ce:
                status = (int)HttpStatusCode.Conflict;
                problem = new ProblemDetails
                {
                    Status = status,
                    Title = "Conflict",
                    Detail = ce.Message,
                    Instance = context.Request.Path
                };
                break;

            case NotFoundException nf:
                status = (int)HttpStatusCode.NotFound;
                problem = new ProblemDetails
                {
                    Status = status,
                    Title = "Not Found",
                    Detail = nf.Message,
                    Instance = context.Request.Path
                };
                break;

            case UnauthorizedException ua:
                status = (int)HttpStatusCode.Unauthorized;
                problem = new ProblemDetails
                {
                    Status = status,
                    Title = "Unauthorized",
                    Detail = ua.Message,
                    Instance = context.Request.Path
                };
                break;

            default:
                _logger.LogError(ex, "Unhandled exception for {Method} {Path}",
                    context.Request.Method, context.Request.Path);
                status = (int)HttpStatusCode.InternalServerError;
                problem = new ProblemDetails
                {
                    Status = status,
                    Title = "An error occurred while processing your request.",
                    Detail = _env.IsDevelopment() ? ex.ToString() : null,
                    Instance = context.Request.Path
                };
                break;
        }

        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = status;

        await context.Response.WriteAsync(JsonSerializer.Serialize(problem, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        }));
    }
}