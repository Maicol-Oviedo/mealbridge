using MealBridge.Domain.Donations;
using MealBridge.Application.Donations.Queries;

namespace MealBridge.Application.Donations.Ports;

public interface IDonationRepository
{
    Task AddAsync(
        DonationLot lot,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<DonationLot>> ListAsync(
        DonationFilters filters,
        CancellationToken cancellationToken = default);

    Task<DonationLot?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task UpdateAsync(
        DonationLot lot,
        CancellationToken cancellationToken = default);
}
