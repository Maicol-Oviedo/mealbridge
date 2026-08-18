using DotNetEnv;

namespace MealBridge.Tests.Configuration;

public static class TestEnvironment
{
    private const string ConnectionVariable =
        "ConnectionStrings__MealBridge";
    private const string MissingConnectionMessage =
        "La conexión PostgreSQL de pruebas no está configurada.";
    private static readonly object SyncRoot = new();
    private static bool isLoaded;

    public static string GetMealBridgeConnectionString()
    {
        EnsureLoaded();
        return Environment.GetEnvironmentVariable(ConnectionVariable)
            ?? throw new InvalidOperationException(
                MissingConnectionMessage);
    }

    private static void EnsureLoaded()
    {
        if (isLoaded)
        {
            return;
        }

        lock (SyncRoot)
        {
            if (isLoaded)
            {
                return;
            }

            Env.NoClobber().TraversePath().Load();
            isLoaded = true;
        }
    }
}
