using MealBridge.Application.Donations.Ports;
using MealBridge.Application.Donations.Queries;
using MealBridge.Application.Donations.UseCases;
using MealBridge.Domain.Donations;
using MealBridge.Domain.Exceptions;

namespace MealBridge.Tests.Application;

public sealed class DonationWorkflowTests
{
    private const string CoordinatorName = "Banco de Alimentos de Bogotá";

    [Fact]
    public async Task Claim_WhenDonationExists_ClaimsAndPersistsDonation()
    {
        var lot = CreateLot();
        var repository = new WorkflowDonationRepository(lot);
        var useCase = new ClaimDonation(repository);

        var result = await useCase.ExecuteAsync(lot.Id, CoordinatorName);

        Assert.Equal(DonationStatus.Claimed, result.Status);
        Assert.Equal(CoordinatorName, result.ClaimedBy);
        Assert.Same(result, repository.UpdatedLot);
    }

    [Fact]
    public async Task Claim_WhenDonationDoesNotExist_ThrowsNotFound()
    {
        var useCase = new ClaimDonation(new WorkflowDonationRepository(null));

        await Assert.ThrowsAsync<NotFoundException>(() =>
            useCase.ExecuteAsync(Guid.NewGuid(), CoordinatorName));
    }

    [Fact]
    public async Task Claim_WhenDonationAlreadyClaimed_ThrowsConflict()
    {
        var lot = CreateLot();
        lot.Claim(CoordinatorName);
        var useCase = new ClaimDonation(new WorkflowDonationRepository(lot));

        await Assert.ThrowsAsync<ConflictException>(() =>
            useCase.ExecuteAsync(lot.Id, "Fundación Comida para Todos"));
    }

    [Fact]
    public async Task ChangeStatus_WhenTransitionIsAllowed_PersistsDonation()
    {
        var lot = CreateLot();
        lot.Claim(CoordinatorName);
        var repository = new WorkflowDonationRepository(lot);
        var useCase = new ChangeDonationStatus(repository);

        var result = await useCase.ExecuteAsync(
            lot.Id,
            DonationStatus.PickedUp);

        Assert.Equal(DonationStatus.PickedUp, result.Status);
        Assert.Same(result, repository.UpdatedLot);
    }

    [Fact]
    public async Task ChangeStatus_WhenDonationDoesNotExist_ThrowsNotFound()
    {
        var useCase = new ChangeDonationStatus(
            new WorkflowDonationRepository(null));

        await Assert.ThrowsAsync<NotFoundException>(() =>
            useCase.ExecuteAsync(Guid.NewGuid(), DonationStatus.Cancelled));
    }

    [Fact]
    public async Task ChangeStatus_WhenTransitionIsIllegal_ThrowsConflict()
    {
        var lot = CreateLot();
        var useCase = new ChangeDonationStatus(
            new WorkflowDonationRepository(lot));

        await Assert.ThrowsAsync<ConflictException>(() =>
            useCase.ExecuteAsync(lot.Id, DonationStatus.PickedUp));
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

    private sealed class WorkflowDonationRepository(
        DonationLot? lot) : IDonationRepository
    {
        public DonationLot? UpdatedLot { get; private set; }

        public Task AddAsync(
            DonationLot donation,
            CancellationToken cancellationToken = default) =>
            Task.CompletedTask;

        public Task<IReadOnlyList<DonationLot>> ListAsync(
            DonationFilters filters,
            CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<DonationLot>>([]);

        public Task<DonationLot?> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(lot?.Id == id ? lot : null);

        public Task UpdateAsync(
            DonationLot donation,
            CancellationToken cancellationToken = default)
        {
            UpdatedLot = donation;
            return Task.CompletedTask;
        }
    }
}
