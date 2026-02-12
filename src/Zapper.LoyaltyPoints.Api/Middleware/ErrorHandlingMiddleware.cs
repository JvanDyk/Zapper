using System.Net;
using System.Text.Json;
using FluentValidation;
using Zapper.LoyaltyPoints.Infrastructure.Models;

namespace Zapper.LoyaltyPoints.Api.Middleware;

public sealed class ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
    };

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var correlationId = context.Items["CorrelationId"]?.ToString();

        var (statusCode, errorCode, message) = exception switch
        {
            ValidationException => (
                StatusCodes.Status400BadRequest,
                "VALIDATION_ERROR",
                "One or more validation errors occurred."
            ),

            DomainValidationException dvex => (
                StatusCodes.Status400BadRequest,
                dvex.Code,
                dvex.Message
            ),

            DuplicateTransactionException => (
                StatusCodes.Status409Conflict,
                "DUPLICATE_TRANSACTION",
                exception.Message
            ),

            ConflictException => (
                StatusCodes.Status409Conflict,
                "CONFLICT",
                "Conflict occurred."
            ),

            EntityNotFoundException => (
                StatusCodes.Status404NotFound,
                "NOT_FOUND",
                "Resource not found."
            ),

            ArgumentException => (
                StatusCodes.Status400BadRequest,
                "BAD_REQUEST",
                "Invalid request."
            ),

            InvalidOperationException => (
                StatusCodes.Status400BadRequest,
                "VALIDATION_ERROR",
                exception.Message
            ),

            OperationCanceledException => (
                StatusCodes.Status400BadRequest,
                "REQUEST_CANCELLED",
                "The request was cancelled."
            ),

            _ => (
                StatusCodes.Status500InternalServerError,
                "INTERNAL_ERROR",
                "An unexpected error occurred."
            )
        };

        logger.LogWarning(
            "[ErrorHandling] {ErrorCode} | CorrelationId={CorrelationId} Method={Method} Path={Path} StatusCode={StatusCode} Message={Message}",
            errorCode, correlationId, context.Request.Method, context.Request.Path, statusCode, exception.Message);

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = statusCode;

        var response = new ErrorResponse(errorCode, message);
        var json = JsonSerializer.Serialize(response, JsonOptions);
        await context.Response.WriteAsync(json);
    }
}
