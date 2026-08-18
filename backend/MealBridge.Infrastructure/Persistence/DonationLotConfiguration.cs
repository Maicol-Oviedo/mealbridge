using MealBridge.Domain.Donations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MealBridge.Infrastructure.Persistence;

public sealed class DonationLotConfiguration :
    IEntityTypeConfiguration<DonationLot>
{
    public void Configure(EntityTypeBuilder<DonationLot> builder)
    {
        builder.ToTable("donation_lots");
        builder.HasKey(lot => lot.Id);

        builder.Property(lot => lot.Id).HasColumnName("id");
        builder.Property(lot => lot.BusinessName)
            .HasColumnName("business_name")
            .HasMaxLength(120)
            .IsRequired();
        builder.Property(lot => lot.Title)
            .HasColumnName("title")
            .HasMaxLength(80)
            .IsRequired();
        builder.Property(lot => lot.Description)
            .HasColumnName("description")
            .HasMaxLength(500);
        builder.Property(lot => lot.FoodCategory)
            .HasColumnName("food_category")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();
        builder.Property(lot => lot.Quantity)
            .HasColumnName("quantity")
            .IsRequired();
        builder.Property(lot => lot.Unit)
            .HasColumnName("unit")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();
        builder.Property(lot => lot.PickupAddress)
            .HasColumnName("pickup_address")
            .HasMaxLength(200)
            .IsRequired();
        builder.Property(lot => lot.AvailableFrom)
            .HasColumnName("available_from")
            .IsRequired();
        builder.Property(lot => lot.AvailableUntil)
            .HasColumnName("available_until")
            .IsRequired();
        builder.Property(lot => lot.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsConcurrencyToken()
            .IsRequired();
        builder.Property(lot => lot.ClaimedBy)
            .HasColumnName("claimed_by")
            .HasMaxLength(120);
        builder.Property(lot => lot.ClaimedAt)
            .HasColumnName("claimed_at");
        builder.Property(lot => lot.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();
        builder.Property(lot => lot.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();
    }
}
