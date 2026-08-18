using MealBridge.Domain.Donations;
using BusinessConflictException =
    MealBridge.Domain.Exceptions.ConflictException;
using BusinessInvalidArgumentException =
    MealBridge.Domain.Exceptions.InvalidArgumentException;

namespace MealBridge.Tests.Domain;

public sealed class DonationLotClaimTests
{
    private const string BusinessName = "Panadería El Nogal";
    private const string Title = "Baguettes del día anterior";
    private const string Description =
        "Sirven todavía para tostadas y crotones.";
    private const string PickupAddress = "Calle 79 #11-45, Bogotá";
    private const string CoordinatorName =
        "Banco de Alimentos de Bogotá";
    private const string SecondCoordinatorName =
        "Fundación Comida para Todos";
    private const int Quantity = 24;
    private const string CoordinatorNameRequiredMessage =
        "El nombre del coordinador es obligatorio.";
    private const string CoordinatorNameTooLongMessage =
        "El nombre del coordinador no puede superar 120 caracteres.";
    private const string ClaimConflictMessage =
        "El lote solo puede reclamarse cuando está disponible.";

    private static readonly DateTimeOffset AvailableFrom =
        new(2026, 8, 18, 16, 0, 0, TimeSpan.Zero);

    private static readonly DateTimeOffset AvailableUntil =
        new(2026, 8, 18, 20, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Claim_WhenAvailable_SetsClaimedByAndStatus()
    {
        var lot = CreateLot();
        var previousUpdatedAt = lot.UpdatedAt;

        lot.Claim(CoordinatorName);

        Assert.Equal(DonationStatus.Claimed, lot.Status);
        Assert.Equal(CoordinatorName, lot.ClaimedBy);
        Assert.NotNull(lot.ClaimedAt);
        Assert.Equal(lot.ClaimedAt, lot.UpdatedAt);
        Assert.True(lot.UpdatedAt > previousUpdatedAt);
    }

    [Fact]
    public void Claim_WhenCoordinatorNameIsBlank_Rejects()
    {
        var lot = CreateLot();

        var exception = Assert.Throws<BusinessInvalidArgumentException>(() =>
            lot.Claim(" "));

        Assert.Equal(CoordinatorNameRequiredMessage, exception.Message);
    }

    [Fact]
    public void Claim_WhenCoordinatorNameExceedsMaximumLength_Rejects()
    {
        var lot = CreateLot();

        var exception = Assert.Throws<BusinessInvalidArgumentException>(() =>
            lot.Claim(new string('a', 121)));

        Assert.Equal(CoordinatorNameTooLongMessage, exception.Message);
    }

    [Fact]
    public void Claim_WhenAlreadyClaimed_Conflicts()
    {
        var lot = CreateLot();
        lot.Claim(CoordinatorName);
        var firstClaimedAt = lot.ClaimedAt;
        var firstUpdatedAt = lot.UpdatedAt;

        var exception = Assert.Throws<BusinessConflictException>(() =>
            lot.Claim(SecondCoordinatorName));

        Assert.Equal(ClaimConflictMessage, exception.Message);
        Assert.Equal(DonationStatus.Claimed, lot.Status);
        Assert.Equal(CoordinatorName, lot.ClaimedBy);
        Assert.Equal(firstClaimedAt, lot.ClaimedAt);
        Assert.Equal(firstUpdatedAt, lot.UpdatedAt);
    }

    private static DonationLot CreateLot() =>
        DonationLot.Create(
            businessName: BusinessName,
            title: Title,
            description: Description,
            foodCategory: FoodCategory.Bakery,
            quantity: Quantity,
            unit: DonationUnit.Loaves,
            pickupAddress: PickupAddress,
            availableFrom: AvailableFrom,
            availableUntil: AvailableUntil);
}
