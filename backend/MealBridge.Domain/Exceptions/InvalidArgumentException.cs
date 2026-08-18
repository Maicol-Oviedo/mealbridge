namespace MealBridge.Domain.Exceptions;

public sealed class InvalidArgumentException(
    string message,
    Exception? innerException = null)
    : Exception(message, innerException);
