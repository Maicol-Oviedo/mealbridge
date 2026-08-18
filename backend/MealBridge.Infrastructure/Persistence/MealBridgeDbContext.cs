using MealBridge.Domain.Donations;
using Microsoft.EntityFrameworkCore;

namespace MealBridge.Infrastructure.Persistence;

public sealed class MealBridgeDbContext(
    DbContextOptions<MealBridgeDbContext> options) : DbContext(options)
{
    public DbSet<DonationLot> DonationLots => Set<DonationLot>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new DonationLotConfiguration());
    }
}
