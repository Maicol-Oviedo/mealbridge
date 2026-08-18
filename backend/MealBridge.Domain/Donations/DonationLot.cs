using MealBridge.Domain.Exceptions;

namespace MealBridge.Domain.Donations;

public sealed class DonationLot
{
    private const string InvalidQuantityMessage =
        "La cantidad debe ser mayor o igual a 1.";
    private const int BusinessNameMaxLength = 120;
    private const int TitleMaxLength = 80;
    private const int DescriptionMaxLength = 500;
    private const int PickupAddressMaxLength = 200;
    private const int CoordinatorNameMaxLength = 120;
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
    private const string CoordinatorNameRequiredMessage =
        "El nombre del coordinador es obligatorio.";
    private const string CoordinatorNameTooLongMessage =
        "El nombre del coordinador no puede superar 120 caracteres.";
    private const string ClaimConflictMessage =
        "El lote solo puede reclamarse cuando está disponible.";
    private const string InvalidTransitionMessage =
        "La transición de estado solicitada no está permitida.";

    private DonationLot(
        Guid id,
        string businessName,
        string title,
        string? description,
        FoodCategory foodCategory,
        int quantity,
        DonationUnit unit,
        string pickupAddress,
        DateTimeOffset availableFrom,
        DateTimeOffset availableUntil,
        DonationStatus status,
        string? claimedBy,
        DateTimeOffset? claimedAt,
        DateTimeOffset createdAt,
        DateTimeOffset updatedAt)
    {
        Id = id;
        BusinessName = businessName;
        Title = title;
        Description = description;
        FoodCategory = foodCategory;
        Quantity = quantity;
        Unit = unit;
        PickupAddress = pickupAddress;
        AvailableFrom = availableFrom;
        AvailableUntil = availableUntil;
        Status = status;
        ClaimedBy = claimedBy;
        ClaimedAt = claimedAt;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
    }

    public Guid Id { get; }
    public string BusinessName { get; }
    public string Title { get; }
    public string? Description { get; }
    public FoodCategory FoodCategory { get; }
    public int Quantity { get; }
    public DonationUnit Unit { get; }
    public string PickupAddress { get; }
    public DateTimeOffset AvailableFrom { get; }
    public DateTimeOffset AvailableUntil { get; }
    public DonationStatus Status { get; private set; }
    public string? ClaimedBy { get; private set; }
    public DateTimeOffset? ClaimedAt { get; private set; }
    public DateTimeOffset CreatedAt { get; }
    public DateTimeOffset UpdatedAt { get; private set; }

    public static DonationLot Create(
        string businessName,
        string title,
        string? description,
        FoodCategory foodCategory,
        int quantity,
        DonationUnit unit,
        string pickupAddress,
        DateTimeOffset availableFrom,
        DateTimeOffset availableUntil)
    {
        if (quantity < 1)
        {
            throw new InvalidArgumentException(InvalidQuantityMessage);
        }

        ValidateRequiredText(
            businessName,
            BusinessNameMaxLength,
            BusinessNameRequiredMessage,
            BusinessNameTooLongMessage);

        ValidateRequiredText(
            title,
            TitleMaxLength,
            TitleRequiredMessage,
            TitleTooLongMessage);

        ValidateOptionalText(
            description,
            DescriptionMaxLength,
            DescriptionTooLongMessage);

        ValidateRequiredText(
            pickupAddress,
            PickupAddressMaxLength,
            PickupAddressRequiredMessage,
            PickupAddressTooLongMessage);

        ValidateDefinedEnum(
            foodCategory,
            InvalidFoodCategoryMessage);

        ValidateDefinedEnum(
            unit,
            InvalidDonationUnitMessage);

        if (availableUntil <= availableFrom)
        {
            throw new InvalidArgumentException(
                InvalidAvailabilityWindowMessage);
        }

        var utcAvailableFrom = availableFrom.ToUniversalTime();
        var utcAvailableUntil = availableUntil.ToUniversalTime();
        var now = DateTimeOffset.UtcNow;

        return new DonationLot(
            Guid.NewGuid(),
            businessName,
            title,
            description,
            foodCategory,
            quantity,
            unit,
            pickupAddress,
            utcAvailableFrom,
            utcAvailableUntil,
            DonationStatus.Available,
            null,
            null,
            now,
            now);
    }

    public void Claim(string coordinatorName)
    {
        ValidateRequiredText(
            coordinatorName,
            CoordinatorNameMaxLength,
            CoordinatorNameRequiredMessage,
            CoordinatorNameTooLongMessage);

        if (Status != DonationStatus.Available)
        {
            throw new ConflictException(ClaimConflictMessage);
        }

        var claimedAt = GetNextMutationTimestamp();

        Status = DonationStatus.Claimed;
        ClaimedBy = coordinatorName;
        ClaimedAt = claimedAt;
        UpdatedAt = claimedAt;
    }

    public void ChangeStatus(DonationStatus targetStatus)
    {
        if (!DonationStatusTransitions.IsAllowed(Status, targetStatus))
        {
            throw new ConflictException(InvalidTransitionMessage);
        }

        Status = targetStatus;
        UpdatedAt = GetNextMutationTimestamp();
    }

    private static void ValidateRequiredText(
        string? value,
        int maxLength,
        string requiredMessage,
        string tooLongMessage)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidArgumentException(requiredMessage);
        }

        if (value.Length > maxLength)
        {
            throw new InvalidArgumentException(tooLongMessage);
        }
    }

    private static void ValidateOptionalText(
        string? value,
        int maxLength,
        string tooLongMessage)
    {
        if (value is not null && value.Length > maxLength)
        {
            throw new InvalidArgumentException(tooLongMessage);
        }
    }

    private static void ValidateDefinedEnum<TEnum>(
        TEnum value,
        string invalidMessage)
        where TEnum : struct, Enum
    {
        if (!Enum.IsDefined(value))
        {
            throw new InvalidArgumentException(invalidMessage);
        }
    }

    private DateTimeOffset GetNextMutationTimestamp()
    {
        var timestamp = DateTimeOffset.UtcNow;
        return timestamp > UpdatedAt
            ? timestamp
            : UpdatedAt.AddTicks(1);
    }
}
