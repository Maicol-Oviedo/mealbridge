using MealBridge.Application.Donations.Commands;
using MealBridge.Application.Donations.Ports;
using MealBridge.Domain.Donations;

namespace MealBridge.Application.Donations.UseCases;

public sealed class CreateDonation(IDonationRepository repository)
{
    public async Task<DonationLot> ExecuteAsync(
        CreateDonationCommand command,
        CancellationToken cancellationToken = default)
    {
        var lot = DonationLot.Create(
            command.BusinessName,
            command.Title,
            command.Description,
            command.FoodCategory,
            command.Quantity,
            command.Unit,
            command.PickupAddress,
            command.AvailableFrom,
            command.AvailableUntil);

        await repository.AddAsync(lot, cancellationToken);
        return lot;
    }
}
