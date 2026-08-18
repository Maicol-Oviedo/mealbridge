using MealBridge.Domain.Donations;

namespace MealBridge.Api.Contracts.Donations;

public sealed record DonationResponse(
    Guid Id,
    string BusinessName,
    string Title,
    string? Description,
    FoodCategory FoodCategory,
    int Quantity,
    DonationUnit Unit,
    string PickupAddress,
    DateTimeOffset AvailableFrom,
    DateTimeOffset AvailableUntil,
    DonationStatus Status,
    string? ClaimedBy,
    DateTimeOffset? ClaimedAt,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt)
{
    public static DonationResponse FromDomain(DonationLot lot) =>
        new(
            lot.Id,
            lot.BusinessName,
            lot.Title,
            lot.Description,
            lot.FoodCategory,
            lot.Quantity,
            lot.Unit,
            lot.PickupAddress,
            lot.AvailableFrom,
            lot.AvailableUntil,
            lot.Status,
            lot.ClaimedBy,
            lot.ClaimedAt,
            lot.CreatedAt,
            lot.UpdatedAt);
}
