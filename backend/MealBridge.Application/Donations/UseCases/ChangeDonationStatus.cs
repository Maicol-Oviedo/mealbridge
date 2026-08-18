using MealBridge.Application.Donations.Ports;
using MealBridge.Domain.Donations;
using MealBridge.Domain.Exceptions;

namespace MealBridge.Application.Donations.UseCases;

public sealed class ChangeDonationStatus(IDonationRepository repository)
{
    private const string DonationNotFoundMessage =
        "No se encontró el lote de donación solicitado.";

    public async Task<DonationLot> ExecuteAsync(
        Guid id,
        DonationStatus targetStatus,
        CancellationToken cancellationToken = default)
    {
        var lot = await repository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(DonationNotFoundMessage);

        lot.ChangeStatus(targetStatus);
        await repository.UpdateAsync(lot, cancellationToken);
        return lot;
    }
}
