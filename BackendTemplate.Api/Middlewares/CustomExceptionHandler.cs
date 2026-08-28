using BackendTemplate.Application.Common.Exceptions;
using BackendTemplate.Domain.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace BackendTemplate.Api.Middlewares;

public class CustomExceptionHandler : IExceptionHandler
{
    private readonly ILogger<CustomExceptionHandler> _logger;

    public CustomExceptionHandler(ILogger<CustomExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        _logger.LogError(exception, "Unhandled exception occurred: {Message}", exception.Message);

        var (statusCode, problemDetails) = exception switch
        {
            ValidationException validationEx => (
                StatusCodes.Status400BadRequest,
                new ValidationProblemDetails(validationEx.Errors)
                {
                    Status = StatusCodes.Status400BadRequest,
                    Title = "Validation Error",
                    Detail = "One or more validation failures occurred.",
                    Instance = httpContext.Request.Path
                }),

            NotFoundException notFoundEx => (
                StatusCodes.Status404NotFound,
                new ProblemDetails
                {
                    Status = StatusCodes.Status404NotFound,
                    Title = "Resource Not Found",
                    Detail = notFoundEx.Message,
                    Instance = httpContext.Request.Path
                }),

            UnauthorizedException unauthorizedEx => (
                StatusCodes.Status401Unauthorized,
                new ProblemDetails
                {
                    Status = StatusCodes.Status401Unauthorized,
                    Title = "Unauthorized",
                    Detail = unauthorizedEx.Message,
                    Instance = httpContext.Request.Path
                }),

            ForbiddenException forbiddenEx => (
                StatusCodes.Status403Forbidden,
                new ProblemDetails
                {
                    Status = StatusCodes.Status403Forbidden,
                    Title = "Forbidden",
                    Detail = forbiddenEx.Message,
                    Instance = httpContext.Request.Path
                }),

            DomainException domainEx => (
                StatusCodes.Status400BadRequest,
                new ProblemDetails
                {
                    Status = StatusCodes.Status400BadRequest,
                    Title = "Domain Rule Violation",
                    Detail = domainEx.Message,
                    Instance = httpContext.Request.Path
                }),

            _ => (
                StatusCodes.Status500InternalServerError,
                new ProblemDetails
                {
                    Status = StatusCodes.Status500InternalServerError,
                    Title = "Internal Server Error",
                    Detail = "An unexpected error occurred. Please try again later.",
                    Instance = httpContext.Request.Path
                })
        };

        httpContext.Response.StatusCode = statusCode;
        httpContext.Response.ContentType = "application/problem+json";

        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }
}
