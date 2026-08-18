using MealBridge.Application.Donations.Ports;
using MealBridge.Application.Donations.Queries;
using MealBridge.Domain.Donations;

namespace MealBridge.Application.Donations.UseCases;

public sealed class ListDonations(IDonationRepository repository)
{
    public Task<IReadOnlyList<DonationLot>> ExecuteAsync(
        DonationFilters filters,
        CancellationToken cancellationToken = default) =>
        repository.ListAsync(filters, cancellationToken);
}
