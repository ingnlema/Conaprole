using Conaprole.Orders.Domain.PointsOfSale;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Conaprole.Orders.Infrastructure.Configuration;

public sealed class PointOfSaleConfiguration : IEntityTypeConfiguration<PointOfSale>
{
    public void Configure(EntityTypeBuilder<PointOfSale> builder)
    {
        builder.ToTable("point_of_sale");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Name)
            .HasColumnName("name")
            .IsRequired();

        builder.Property(p => p.PhoneNumber)
            .HasColumnName("phone_number")
            .IsRequired();

        builder.Property(p => p.Address)
            .HasColumnName("address")
            .IsRequired();

        builder.Property(p => p.IsActive)
            .HasColumnName("is_active")
            .IsRequired();

        builder.Property(p => p.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();
    }
}