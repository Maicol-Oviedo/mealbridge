namespace MealBridge.Api.Contracts;

public sealed record ApiEnvelope<T>(
    bool Succeeded,
    T? Data,
    string? Error)
{
    public static ApiEnvelope<T> Success(T data) =>
        new(true, data, null);

    public static ApiEnvelope<T> Failure(string error) =>
        new(false, default, error);
}
