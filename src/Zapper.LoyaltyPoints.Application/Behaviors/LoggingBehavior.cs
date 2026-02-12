using System.Diagnostics;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace Zapper.LoyaltyPoints.Application.Behaviors;

public sealed class LoggingBehavior<TRequest, TResponse>(
    ILogger<LoggingBehavior<TRequest, TResponse>> logger) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var requestJson = JsonSerializer.Serialize(request);
        var stopwatch = Stopwatch.StartNew();

        try
        {
            var response = await next();
            stopwatch.Stop();

            logger.LogInformation("[MediatR] Handled {RequestName} in {ElapsedMs}ms. Request: {Request}",
                requestName, stopwatch.ElapsedMilliseconds, requestJson);

            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            logger.LogError(ex, "[MediatR] {RequestName} failed after {ElapsedMs}ms. Request: {Request}",
                requestName, stopwatch.ElapsedMilliseconds, requestJson);
            throw;
        }
    }
}
