using MealBridge.Domain.Donations;

namespace MealBridge.Api.Contracts.Donations;

public sealed record ChangeDonationStatusRequest(DonationStatus? Status);
