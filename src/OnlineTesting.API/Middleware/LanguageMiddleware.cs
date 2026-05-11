using OnlineTesting.API.Localization;
using OnlineTesting.Application.Common.Interfaces;

namespace OnlineTesting.API.Middleware;

public class LanguageMiddleware
{
    private readonly RequestDelegate _next;

    public LanguageMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext ctx, ILanguageContext lang)
    {
        if (lang is LanguageContext concrete)
            concrete.SetFromRequest(ctx);

        await _next(ctx);
    }
}