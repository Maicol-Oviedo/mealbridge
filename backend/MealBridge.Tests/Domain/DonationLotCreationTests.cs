using MealBridge.Domain.Donations;
using BusinessInvalidArgumentException =
    MealBridge.Domain.Exceptions.InvalidArgumentException;

namespace MealBridge.Tests.Domain;

public sealed class DonationLotCreationTests
{
    private const string BusinessName = "Panadería El Nogal";
    private const string Title = "Baguettes del día anterior";
    private const string Description =
        "Sirven todavía para tostadas y crotones.";
    private const int Quantity = 24;
    private const string PickupAddress = "Calle 79 #11-45, Bogotá";
    private const string InvalidQuantityMessage =
        "La cantidad debe ser mayor o igual a 1.";
    private const string BusinessNameRequiredMessage =
        "El nombre del negocio es obligatorio.";
    private const string BusinessNameTooLongMessage =
        "El nombre del negocio no puede superar 120 caracteres.";
    private const string TitleRequiredMessage =
        "El título es obligatorio.";
    private const string TitleTooLongMessage =
        "El título no puede superar 80 caracteres.";
    private const string DescriptionTooLongMessage =
        "La descripción no puede superar 500 caracteres.";
    private const string PickupAddressRequiredMessage =
        "La dirección de recogida es obligatoria.";
    private const string PickupAddressTooLongMessage =
        "La dirección de recogida no puede superar 200 caracteres.";
    private const string InvalidFoodCategoryMessage =
        "La categoría de alimento no es válida.";
    private const string InvalidDonationUnitMessage =
        "La unidad de donación no es válida.";
    private const string InvalidAvailabilityWindowMessage =
        "La fecha final de disponibilidad debe ser posterior a la fecha inicial.";
    private const FoodCategory InvalidFoodCategory = (FoodCategory)(-1);
    private const DonationUnit InvalidDonationUnit = (DonationUnit)(-1);

    private static readonly DateTimeOffset AvailableFrom =
        new(2026, 8, 18, 16, 0, 0, TimeSpan.Zero);

    private static readonly DateTimeOffset AvailableUntil =
        new(2026, 8, 18, 20, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Create_WhenValid_ReturnsAvailableLot()
    {
        var lot = DonationLot.Create(
            businessName: BusinessName,
            title: Title,
            description: Description,
            foodCategory: FoodCategory.Bakery,
            quantity: Quantity,
            unit: DonationUnit.Loaves,
            pickupAddress: PickupAddress,
            availableFrom: AvailableFrom,
            availableUntil: AvailableUntil);

        Assert.NotEqual(Guid.Empty, lot.Id);
        Assert.Equal(BusinessName, lot.BusinessName);
        Assert.Equal(Title, lot.Title);
        Assert.Equal(Description, lot.Description);
        Assert.Equal(FoodCategory.Bakery, lot.FoodCategory);
        Assert.Equal(Quantity, lot.Quantity);
        Assert.Equal(DonationUnit.Loaves, lot.Unit);
        Assert.Equal(PickupAddress, lot.PickupAddress);
        Assert.Equal(AvailableFrom, lot.AvailableFrom);
        Assert.Equal(AvailableUntil, lot.AvailableUntil);
        Assert.Equal(DonationStatus.Available, lot.Status);
        Assert.Null(lot.ClaimedBy);
        Assert.Null(lot.ClaimedAt);
        Assert.NotEqual(default, lot.CreatedAt);
        Assert.Equal(lot.CreatedAt, lot.UpdatedAt);
    }

    [Fact]
    public void Create_WhenQuantityLessThanOne_Rejects()
    {
        var exception = Assert.Throws<BusinessInvalidArgumentException>(() =>
            DonationLot.Create(
                businessName: BusinessName,
                title: Title,
                description: Description,
                foodCategory: FoodCategory.Bakery,
                quantity: 0,
                unit: DonationUnit.Loaves,
                pickupAddress: PickupAddress,
                availableFrom: AvailableFrom,
                availableUntil: AvailableUntil));

        Assert.Equal(InvalidQuantityMessage, exception.Message);
    }

    [Fact]
    public void Create_WhenBusinessNameIsBlank_Rejects()
    {
        var exception = Assert.Throws<BusinessInvalidArgumentException>(() =>
            CreateLot(businessName: " "));

        Assert.Equal(BusinessNameRequiredMessage, exception.Message);
    }

    [Fact]
    public void Create_WhenBusinessNameExceedsMaximumLength_Rejects()
    {
        var exception = Assert.Throws<BusinessInvalidArgumentException>(() =>
            CreateLot(businessName: new string('a', 121)));

        Assert.Equal(BusinessNameTooLongMessage, exception.Message);
    }

    [Fact]
    public void Create_WhenTitleIsBlank_Rejects()
    {
        var exception = Assert.Throws<BusinessInvalidArgumentException>(() =>
            CreateLot(title: " "));

        Assert.Equal(TitleRequiredMessage, exception.Message);
    }

    [Fact]
    public void Create_WhenTitleExceedsMaximumLength_Rejects()
    {
        var exception = Assert.Throws<BusinessInvalidArgumentException>(() =>
            CreateLot(title: new string('a', 81)));

        Assert.Equal(TitleTooLongMessage, exception.Message);
    }

    [Fact]
    public void Create_WhenDescriptionExceedsMaximumLength_Rejects()
    {
        var exception = Assert.Throws<BusinessInvalidArgumentException>(() =>
            CreateLot(description: new string('a', 501)));

        Assert.Equal(DescriptionTooLongMessage, exception.Message);
    }

    [Fact]
    public void Create_WhenDescriptionIsNull_Succeeds()
    {
        var lot = CreateLot(description: null);

        Assert.Null(lot.Description);
    }

    [Fact]
    public void Create_WhenPickupAddressIsBlank_Rejects()
    {
        var exception = Assert.Throws<BusinessInvalidArgumentException>(() =>
            CreateLot(pickupAddress: " "));

        Assert.Equal(PickupAddressRequiredMessage, exception.Message);
    }

    [Fact]
    public void Create_WhenPickupAddressExceedsMaximumLength_Rejects()
    {
        var exception = Assert.Throws<BusinessInvalidArgumentException>(() =>
            CreateLot(pickupAddress: new string('a', 201)));

        Assert.Equal(PickupAddressTooLongMessage, exception.Message);
    }

    [Fact]
    public void Create_WhenFoodCategoryIsUndefined_Rejects()
    {
        var exception = Assert.Throws<BusinessInvalidArgumentException>(() =>
            CreateLot(foodCategory: InvalidFoodCategory));

        Assert.Equal(InvalidFoodCategoryMessage, exception.Message);
    }

    [Fact]
    public void Create_WhenDonationUnitIsUndefined_Rejects()
    {
        var exception = Assert.Throws<BusinessInvalidArgumentException>(() =>
            CreateLot(unit: InvalidDonationUnit));

        Assert.Equal(InvalidDonationUnitMessage, exception.Message);
    }

    [Fact]
    public void Create_WhenAvailableUntilIsNotAfterAvailableFrom_Rejects()
    {
        var exception = Assert.Throws<BusinessInvalidArgumentException>(() =>
            CreateLot(availableUntil: AvailableFrom));

        Assert.Equal(InvalidAvailabilityWindowMessage, exception.Message);
    }

    [Fact]
    public void Create_WhenAvailabilityHasOffset_StoresUtcValues()
    {
        var offset = TimeSpan.FromHours(-5);
        var availableFrom =
            new DateTimeOffset(2026, 8, 18, 11, 0, 0, offset);
        var availableUntil =
            new DateTimeOffset(2026, 8, 18, 15, 0, 0, offset);

        var lot = CreateLot(
            availableFrom: availableFrom,
            availableUntil: availableUntil);

        Assert.Equal(TimeSpan.Zero, lot.AvailableFrom.Offset);
        Assert.Equal(TimeSpan.Zero, lot.AvailableUntil.Offset);
        Assert.Equal(availableFrom.UtcDateTime, lot.AvailableFrom.UtcDateTime);
        Assert.Equal(
            availableUntil.UtcDateTime,
            lot.AvailableUntil.UtcDateTime);
    }

    private static DonationLot CreateLot(
        string businessName = BusinessName,
        string title = Title,
        string? description = Description,
        string pickupAddress = PickupAddress,
        FoodCategory foodCategory = FoodCategory.Bakery,
        DonationUnit unit = DonationUnit.Loaves,
        DateTimeOffset? availableFrom = null,
        DateTimeOffset? availableUntil = null) =>
        DonationLot.Create(
            businessName: businessName,
            title: title,
            description: description,
            foodCategory: foodCategory,
            quantity: Quantity,
            unit: unit,
            pickupAddress: pickupAddress,
            availableFrom: availableFrom ?? AvailableFrom,
            availableUntil: availableUntil ?? AvailableUntil);
}
