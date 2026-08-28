using System.Diagnostics;
using BackendTemplate.Application.Common.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BackendTemplate.Application.Common.Behaviors;

public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;
    private readonly ICurrentUserService _currentUserService;

    public LoggingBehavior(
        ILogger<LoggingBehavior<TRequest, TResponse>> logger,
        ICurrentUserService currentUserService)
    {
        _logger = logger;
        _currentUserService = currentUserService;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var userId = _currentUserService.UserId ?? "Anonymous";

        _logger.LogInformation("Beginning request: {RequestName} from user {UserId}", requestName, userId);

        var stopwatch = Stopwatch.StartNew();
        var response = await next();
        stopwatch.Stop();

        _logger.LogInformation("Completed request: {RequestName} for user {UserId} in {ElapsedMilliseconds} ms",
            requestName, userId, stopwatch.ElapsedMilliseconds);

        return response;
    }
}
