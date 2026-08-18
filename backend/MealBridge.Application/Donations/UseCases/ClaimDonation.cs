using MealBridge.Application.Donations.Ports;
using MealBridge.Domain.Donations;
using MealBridge.Domain.Exceptions;

namespace MealBridge.Application.Donations.UseCases;

public sealed class ClaimDonation(IDonationRepository repository)
{
    private const string DonationNotFoundMessage =
        "No se encontró el lote de donación solicitado.";

    public async Task<DonationLot> ExecuteAsync(
        Guid id,
        string coordinatorName,
        CancellationToken cancellationToken = default)
    {
        var lot = await repository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(DonationNotFoundMessage);

        lot.Claim(coordinatorName);
        await repository.UpdateAsync(lot, cancellationToken);
        return lot;
    }
}
