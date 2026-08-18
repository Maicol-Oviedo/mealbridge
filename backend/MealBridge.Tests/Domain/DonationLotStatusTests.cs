using MealBridge.Domain.Donations;
using BusinessConflictException =
    MealBridge.Domain.Exceptions.ConflictException;

namespace MealBridge.Tests.Domain;

public sealed class DonationLotStatusTests
{
    private const string BusinessName = "Panadería El Nogal";
    private const string Title = "Baguettes del día anterior";
    private const string PickupAddress = "Calle 79 #11-45, Bogotá";
    private const string CoordinatorName =
        "Banco de Alimentos de Bogotá";
    private const string InvalidTransitionMessage =
        "La transición de estado solicitada no está permitida.";

    [Fact]
    public void ChangeStatus_WhenAvailableToPickedUp_Conflicts()
    {
        var lot = CreateLot();

        var exception = Assert.Throws<BusinessConflictException>(() =>
            lot.ChangeStatus(DonationStatus.PickedUp));

        Assert.Equal(InvalidTransitionMessage, exception.Message);
        Assert.Equal(DonationStatus.Available, lot.Status);
    }

    [Fact]
    public void ChangeStatus_WhenClaimedToPickedUp_Succeeds()
    {
        var lot = CreateClaimedLot();
        var previousUpdatedAt = lot.UpdatedAt;

        lot.ChangeStatus(DonationStatus.PickedUp);

        Assert.Equal(DonationStatus.PickedUp, lot.Status);
        Assert.True(lot.UpdatedAt > previousUpdatedAt);
    }

    [Fact]
    public void ChangeStatus_WhenAvailableToCancelled_Succeeds()
    {
        var lot = CreateLot();

        lot.ChangeStatus(DonationStatus.Cancelled);

        Assert.Equal(DonationStatus.Cancelled, lot.Status);
    }

    [Fact]
    public void ChangeStatus_WhenClaimedToCancelled_Succeeds()
    {
        var lot = CreateClaimedLot();

        lot.ChangeStatus(DonationStatus.Cancelled);

        Assert.Equal(DonationStatus.Cancelled, lot.Status);
    }

    [Fact]
    public void ChangeStatus_WhenTerminalStateChanges_Conflicts()
    {
        var lot = CreateClaimedLot();
        lot.ChangeStatus(DonationStatus.PickedUp);

        var exception = Assert.Throws<BusinessConflictException>(() =>
            lot.ChangeStatus(DonationStatus.Cancelled));

        Assert.Equal(InvalidTransitionMessage, exception.Message);
        Assert.Equal(DonationStatus.PickedUp, lot.Status);
    }

    private static DonationLot CreateClaimedLot()
    {
        var lot = CreateLot();
        lot.Claim(CoordinatorName);
        return lot;
    }

    private static DonationLot CreateLot() =>
        DonationLot.Create(
            businessName: BusinessName,
            title: Title,
            description: null,
            foodCategory: FoodCategory.Bakery,
            quantity: 24,
            unit: DonationUnit.Loaves,
            pickupAddress: PickupAddress,
            availableFrom: DateTimeOffset.UtcNow.AddHours(1),
            availableUntil: DateTimeOffset.UtcNow.AddHours(5));
}
