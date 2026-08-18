using MealBridge.Application.Donations.Ports;
using MealBridge.Application.Donations.Queries;
using MealBridge.Domain.Donations;
using MealBridge.Domain.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace MealBridge.Infrastructure.Persistence.Repositories;

public sealed class PostgresDonationRepository(
    MealBridgeDbContext dbContext) : IDonationRepository
{
    private const string ConcurrentUpdateMessage =
        "El lote fue modificado por otra operación.";

    public async Task AddAsync(
        DonationLot lot,
        CancellationToken cancellationToken = default)
    {
        dbContext.DonationLots.Add(lot);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<DonationLot>> ListAsync(
        DonationFilters filters,
        CancellationToken cancellationToken = default)
    {
        IQueryable<DonationLot> query = dbContext.DonationLots
            .AsNoTracking();

        if (filters.Status is { } status)
        {
            query = query.Where(lot => lot.Status == status);
        }

        if (filters.FoodCategory is { } foodCategory)
        {
            query = query.Where(lot => lot.FoodCategory == foodCategory);
        }

        return await query
            .OrderByDescending(lot => lot.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public Task<DonationLot?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default) =>
        dbContext.DonationLots.SingleOrDefaultAsync(
            lot => lot.Id == id,
            cancellationToken);

    public async Task UpdateAsync(
        DonationLot lot,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException exception)
        {
            throw new ConflictException(
                ConcurrentUpdateMessage,
                exception);
        }
    }
}
