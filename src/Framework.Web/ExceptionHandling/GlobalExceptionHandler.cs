using System.Diagnostics;
using FluentValidation;
using Framework.Domain.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Framework.Web.ExceptionHandling;

public sealed class GlobalExceptionHandler(
    ILogger<GlobalExceptionHandler> logger,
    IProblemDetailsService problemDetailsService,
    IHostEnvironment environment) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var (statusCode, title) = Map(exception);
        var traceId = Activity.Current?.Id ?? httpContext.TraceIdentifier;
        var path = httpContext.Request.Path.Value;

        if (statusCode >= StatusCodes.Status500InternalServerError)
        {
            logger.LogError(
                exception,
                "Unhandled exception {StatusCode} for {Method} {Path}. TraceId: {TraceId}",
                statusCode,
                httpContext.Request.Method,
                path,
                traceId);
        }
        else
        {
            logger.LogWarning(
                exception,
                "Request failed {StatusCode} for {Method} {Path}. TraceId: {TraceId}",
                statusCode,
                httpContext.Request.Method,
                path,
                traceId);
        }

        httpContext.Response.StatusCode = statusCode;

        if (!PrefersJson(httpContext))
        {
            return false;
        }

        var problem = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = environment.IsDevelopment() ? exception.Message : null,
            Instance = path
        };

        problem.Extensions["traceId"] = traceId;

        if (exception is ValidationException validationException)
        {
            problem.Extensions["errors"] = validationException.Errors
                .GroupBy(error => error.PropertyName)
                .ToDictionary(
                    group => group.Key,
                    group => group.Select(error => error.ErrorMessage).ToArray());
        }

        await problemDetailsService.WriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = problem
        });

        return true;
    }

    private static (int StatusCode, string Title) Map(Exception exception) => exception switch
    {
        ValidationException => (StatusCodes.Status400BadRequest, "Validation failed"),
        NotFoundException => (StatusCodes.Status404NotFound, "Not found"),
        ForbiddenAccessException => (StatusCodes.Status403Forbidden, "Forbidden"),
        UnauthorizedAccessException => (StatusCodes.Status401Unauthorized, "Unauthorized"),
        _ => (StatusCodes.Status500InternalServerError, "An error occurred")
    };

    private static bool PrefersJson(HttpContext httpContext)
    {
        var accept = httpContext.Request.Headers.Accept.ToString();
        return accept.Contains("application/json", StringComparison.OrdinalIgnoreCase)
            || accept.Contains("application/problem+json", StringComparison.OrdinalIgnoreCase);
    }
}
