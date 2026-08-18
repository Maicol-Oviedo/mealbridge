using MealBridge.Application.Donations.Queries;
using MealBridge.Domain.Donations;
using MealBridge.Domain.Exceptions;
using MealBridge.Infrastructure.Persistence;
using MealBridge.Infrastructure.Persistence.Repositories;
using MealBridge.Tests.Configuration;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace MealBridge.Tests.Infrastructure;

public sealed class PostgresDonationRepositoryTests : IAsyncLifetime
{
    private const string TestDatabaseName = "mealbridge_tests";
    private readonly string connectionString;

    public PostgresDonationRepositoryTests()
    {
        var configured =
            TestEnvironment.GetMealBridgeConnectionString();
        var builder = new NpgsqlConnectionStringBuilder(configured)
        {
            Database = TestDatabaseName
        };
        connectionString = builder.ConnectionString;
    }

    [Fact]
    public async Task Add_WhenContextIsRecreated_PersistsAllFields()
    {
        var lot = CreateLot();
        await using (var writeContext = CreateContext())
        {
            var repository = new PostgresDonationRepository(writeContext);
            await repository.AddAsync(lot);
        }

        await using var readContext = CreateContext();
        var persisted = await new PostgresDonationRepository(readContext)
            .GetByIdAsync(lot.Id);

        Assert.NotNull(persisted);
        Assert.Equal(lot.BusinessName, persisted.BusinessName);
        Assert.Equal(
            lot.AvailableUntil.ToUnixTimeMilliseconds(),
            persisted.AvailableUntil.ToUnixTimeMilliseconds());
        Assert.Equal(lot.Status, persisted.Status);
    }

    [Fact]
    public async Task List_WhenFiltersAreCombined_ReturnsMatchingLots()
    {
        await using var context = CreateContext();
        var repository = new PostgresDonationRepository(context);
        await repository.AddAsync(CreateLot());
        await repository.AddAsync(CreateLot(FoodCategory.Produce));

        var result = await repository.ListAsync(
            new DonationFilters(
                DonationStatus.Available,
                FoodCategory.Bakery));

        Assert.Single(result);
        Assert.Equal(FoodCategory.Bakery, result[0].FoodCategory);
    }

    [Fact]
    public async Task Update_WhenTwoClaimsRace_AllowsOnlyOneSuccess()
    {
        var lot = CreateLot();
        await using (var seedContext = CreateContext())
        {
            await new PostgresDonationRepository(seedContext).AddAsync(lot);
        }

        await using var firstContext = CreateContext();
        await using var secondContext = CreateContext();
        var firstRepository = new PostgresDonationRepository(firstContext);
        var secondRepository = new PostgresDonationRepository(secondContext);
        var firstLot = await firstRepository.GetByIdAsync(lot.Id);
        var secondLot = await secondRepository.GetByIdAsync(lot.Id);
        firstLot!.Claim("Banco de Alimentos Uno");
        secondLot!.Claim("Banco de Alimentos Dos");

        var outcomes = await Task.WhenAll(
            CaptureOutcome(() => firstRepository.UpdateAsync(firstLot)),
            CaptureOutcome(() => secondRepository.UpdateAsync(secondLot)));

        Assert.Single(outcomes, outcome => outcome is null);
        Assert.Single(outcomes, outcome => outcome is ConflictException);

        await using var verifyContext = CreateContext();
        var persisted = await new PostgresDonationRepository(verifyContext)
            .GetByIdAsync(lot.Id);
        Assert.Contains(
            persisted!.ClaimedBy,
            new[] { "Banco de Alimentos Uno", "Banco de Alimentos Dos" });
    }

    public async Task InitializeAsync()
    {
        await using var context = CreateContext();
        await context.Database.EnsureDeletedAsync();
        await context.Database.EnsureCreatedAsync();
    }

    public async Task DisposeAsync()
    {
        await using var context = CreateContext();
        await context.Database.EnsureDeletedAsync();
    }

    private MealBridgeDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<MealBridgeDbContext>()
            .UseNpgsql(connectionString)
            .Options;
        return new MealBridgeDbContext(options);
    }

    private static async Task<Exception?> CaptureOutcome(Func<Task> action)
    {
        try
        {
            await action();
            return null;
        }
        catch (Exception exception)
        {
            return exception;
        }
    }

    private static DonationLot CreateLot(
        FoodCategory category = FoodCategory.Bakery) =>
        DonationLot.Create(
            "Panadería El Nogal",
            "Baguettes del día anterior",
            null,
            category,
            24,
            DonationUnit.Loaves,
            "Calle 79 #11-45, Bogotá",
            DateTimeOffset.UtcNow.AddHours(1),
            DateTimeOffset.UtcNow.AddHours(5));
}
