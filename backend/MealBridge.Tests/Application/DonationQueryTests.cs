using MealBridge.Application.Donations.Ports;
using MealBridge.Application.Donations.Queries;
using MealBridge.Application.Donations.UseCases;
using MealBridge.Domain.Donations;
using MealBridge.Domain.Exceptions;

namespace MealBridge.Tests.Application;

public sealed class DonationQueryTests
{
    [Fact]
    public async Task List_WhenRepositoryIsEmpty_ReturnsEmptyList()
    {
        var repository = new QueryDonationRepository([]);
        var useCase = new ListDonations(repository);

        var result = await useCase.ExecuteAsync(new DonationFilters());

        Assert.Empty(result);
    }

    [Fact]
    public async Task List_WhenStatusAndCategoryAreProvided_CombinesFilters()
    {
        var repository = new QueryDonationRepository([CreateLot()]);
        var useCase = new ListDonations(repository);
        var filters = new DonationFilters(
            DonationStatus.Available,
            FoodCategory.Bakery);

        await useCase.ExecuteAsync(filters);

        Assert.Equal(filters, repository.ReceivedFilters);
    }

    [Fact]
    public async Task Get_WhenDonationExists_ReturnsDonation()
    {
        var lot = CreateLot();
        var repository = new QueryDonationRepository([lot]);
        var useCase = new GetDonation(repository);

        var result = await useCase.ExecuteAsync(lot.Id);

        Assert.Same(lot, result);
    }

    [Fact]
    public async Task Get_WhenDonationDoesNotExist_ThrowsNotFound()
    {
        var repository = new QueryDonationRepository([]);
        var useCase = new GetDonation(repository);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            useCase.ExecuteAsync(Guid.NewGuid()));
    }

    private static DonationLot CreateLot() =>
        DonationLot.Create(
            "Panadería El Nogal",
            "Baguettes del día anterior",
            null,
            FoodCategory.Bakery,
            24,
            DonationUnit.Loaves,
            "Calle 79 #11-45, Bogotá",
            DateTimeOffset.UtcNow.AddHours(1),
            DateTimeOffset.UtcNow.AddHours(5));

    private sealed class QueryDonationRepository(
        IReadOnlyList<DonationLot> lots) : IDonationRepository
    {
        public DonationFilters? ReceivedFilters { get; private set; }

        public Task AddAsync(
            DonationLot lot,
            CancellationToken cancellationToken = default) =>
            Task.CompletedTask;

        public Task<IReadOnlyList<DonationLot>> ListAsync(
            DonationFilters filters,
            CancellationToken cancellationToken = default)
        {
            ReceivedFilters = filters;
            return Task.FromResult(lots);
        }

        public Task<DonationLot?> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(lots.SingleOrDefault(lot => lot.Id == id));

        public Task UpdateAsync(
            DonationLot lot,
            CancellationToken cancellationToken = default) =>
            Task.CompletedTask;
    }
}
