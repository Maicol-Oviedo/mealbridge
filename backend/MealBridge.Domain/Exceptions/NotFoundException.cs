namespace MealBridge.Domain.Exceptions;

public sealed class NotFoundException(
    string message,
    Exception? innerException = null)
    : Exception(message, innerException);
