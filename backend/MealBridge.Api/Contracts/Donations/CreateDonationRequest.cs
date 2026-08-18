using System.Text.Json;
using System.Text.Json.Serialization;
using MealBridge.Domain.Donations;

namespace MealBridge.Api.Contracts.Donations;

public sealed record CreateDonationRequest(
    string? BusinessName,
    string? Title,
    string? Description,
    FoodCategory? FoodCategory,
    int Quantity,
    DonationUnit? Unit,
    string? PickupAddress,
    DateTimeOffset AvailableFrom,
    DateTimeOffset AvailableUntil)
{
    [JsonExtensionData]
    public IDictionary<string, JsonElement>? AdditionalProperties
    {
        get;
        init;
    }

    public bool DefinesStatus =>
        AdditionalProperties?.Keys.Any(
            key => string.Equals(
                key,
                "status",
                StringComparison.OrdinalIgnoreCase)) == true;
}
