using MealBridge.Application.Donations.Ports;
using MealBridge.Domain.Donations;
using MealBridge.Domain.Exceptions;

namespace MealBridge.Application.Donations.UseCases;

public sealed class GetDonation(IDonationRepository repository)
{
    private const string DonationNotFoundMessage =
        "No se encontró el lote de donación solicitado.";

    public async Task<DonationLot> ExecuteAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var lot = await repository.GetByIdAsync(id, cancellationToken);
        return lot ?? throw new NotFoundException(DonationNotFoundMessage);
    }
}
