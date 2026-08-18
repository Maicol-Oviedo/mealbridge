using MealBridge.Domain.Donations;

namespace MealBridge.Application.Donations.Queries;

public sealed record DonationFilters(
    DonationStatus? Status = null,
    FoodCategory? FoodCategory = null);
