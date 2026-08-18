using MealBridge.Domain.Donations;

namespace MealBridge.Application.Donations.Commands;

public sealed record CreateDonationCommand(
    string BusinessName,
    string Title,
    string? Description,
    FoodCategory FoodCategory,
    int Quantity,
    DonationUnit Unit,
    string PickupAddress,
    DateTimeOffset AvailableFrom,
    DateTimeOffset AvailableUntil);
