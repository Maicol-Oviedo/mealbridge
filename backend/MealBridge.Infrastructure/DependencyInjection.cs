using MealBridge.Application.Donations.Ports;
using MealBridge.Infrastructure.Persistence;
using MealBridge.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace MealBridge.Infrastructure;

public static class DependencyInjection
{
    private const string MissingConnectionStringMessage =
        "No se configuró la conexión de MealBridge.";

    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        string? connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                MissingConnectionStringMessage);
        }

        services.AddDbContext<MealBridgeDbContext>(
            options => options.UseNpgsql(connectionString));
        services.AddScoped<IDonationRepository, PostgresDonationRepository>();
        return services;
    }
}
