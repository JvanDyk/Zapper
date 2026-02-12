using System.Diagnostics;

namespace Zapper.LoyaltyPoints.Api.Middleware;

public sealed class RequestHandlingMiddleware(RequestDelegate next, ILogger<RequestHandlingMiddleware> logger)
{
    private const string CorrelationIdHeader = "X-Correlation-Id";

    public async Task InvokeAsync(HttpContext context)
    {
        if (!context.Request.Headers.TryGetValue(CorrelationIdHeader, out var correlationId)
            || string.IsNullOrWhiteSpace(correlationId))
        {
            correlationId = Guid.NewGuid().ToString();
        }

        context.Items["CorrelationId"] = correlationId.ToString();
        context.Response.Headers[CorrelationIdHeader] = correlationId;

        using (context.RequestServices.GetRequiredService<ILoggerFactory>()
            .CreateLogger("CorrelationId")
            .BeginScope(new Dictionary<string, object> { ["CorrelationId"] = correlationId.ToString()! }))
        {
            var method = context.Request.Method;
            var path = context.Request.Path.ToString();
            var query = context.Request.QueryString.ToString();

            logger.LogInformation(
                "[Request] {Method} {Path}{Query}",
                method, path, query);
            var stopwatch = Stopwatch.StartNew();

            await next(context);

            stopwatch.Stop();

            var statusCode = context.Response.StatusCode;
            var elapsed = stopwatch.ElapsedMilliseconds;

            logger.LogInformation(
                "[Response] {Method} {Path} → {StatusCode} in {ElapsedMs}ms",
                method, path, statusCode, elapsed);
        }
    }
}
