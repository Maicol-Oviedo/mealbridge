using MealBridge.Api.Contracts;
using MealBridge.Domain.Exceptions;
using Microsoft.AspNetCore.Diagnostics;

namespace MealBridge.Api.Middleware;

public sealed class GlobalExceptionHandler(
    ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    private const string UnexpectedErrorMessage =
        "Ocurrió un error inesperado.";
    private const string BusinessErrorLogMessage =
        "La solicitud falló por una regla de negocio: {ErrorMessage}";

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var statusCode = exception switch
        {
            InvalidArgumentException => StatusCodes.Status400BadRequest,
            NotFoundException => StatusCodes.Status404NotFound,
            ConflictException => StatusCodes.Status409Conflict,
            _ => StatusCodes.Status500InternalServerError
        };

        var isBusinessError = statusCode != StatusCodes.Status500InternalServerError;
        var errorMessage = isBusinessError
            ? exception.Message
            : UnexpectedErrorMessage;

        if (isBusinessError)
        {
            logger.LogWarning(
                exception,
                BusinessErrorLogMessage,
                exception.Message);
        }
        else
        {
            logger.LogError(exception, UnexpectedErrorMessage);
        }

        httpContext.Response.StatusCode = statusCode;
        await httpContext.Response.WriteAsJsonAsync(
            ApiEnvelope<object>.Failure(errorMessage),
            cancellationToken);

        return true;
    }
}
