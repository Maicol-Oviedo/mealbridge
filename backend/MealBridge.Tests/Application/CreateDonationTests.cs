using MealBridge.Application.Donations.Commands;
using MealBridge.Application.Donations.Ports;
using MealBridge.Application.Donations.Queries;
using MealBridge.Application.Donations.UseCases;
using MealBridge.Domain.Donations;

namespace MealBridge.Tests.Application;

public sealed class CreateDonationTests
{
    [Fact]
    public async Task Execute_WhenValid_PersistsAndReturnsCreatedLot()
    {
        var repository = new RecordingDonationRepository();
        var useCase = new CreateDonation(repository);
        var command = new CreateDonationCommand(
            "Panadería El Nogal",
            "Baguettes del día anterior",
            "Sirven todavía para tostadas y crotones.",
            FoodCategory.Bakery,
            24,
            DonationUnit.Loaves,
            "Calle 79 #11-45, Bogotá",
            new DateTimeOffset(2026, 8, 18, 16, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2026, 8, 18, 20, 0, 0, TimeSpan.Zero));

        var result = await useCase.ExecuteAsync(command);

        Assert.Same(result, repository.AddedLot);
        Assert.Equal(DonationStatus.Available, result.Status);
    }

    private sealed class RecordingDonationRepository : IDonationRepository
    {
        public DonationLot? AddedLot { get; private set; }

        public Task AddAsync(
            DonationLot lot,
            CancellationToken cancellationToken = default)
        {
            AddedLot = lot;
            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<DonationLot>> ListAsync(
            DonationFilters filters,
            CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<DonationLot>>([]);

        public Task<DonationLot?> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default) =>
            Task.FromResult<DonationLot?>(null);

        public Task UpdateAsync(
            DonationLot lot,
            CancellationToken cancellationToken = default) =>
            Task.CompletedTask;
    }
}
