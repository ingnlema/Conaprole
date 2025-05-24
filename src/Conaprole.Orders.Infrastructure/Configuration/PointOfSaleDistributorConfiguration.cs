using Conaprole.Orders.Domain.Distributors;
using Conaprole.Orders.Domain.PointsOfSale;
using Conaprole.Orders.Domain.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Conaprole.Orders.Infrastructure.Configuration;

public sealed class PointOfSaleDistributorConfiguration : IEntityTypeConfiguration<PointOfSaleDistributor>
{
    public void Configure(EntityTypeBuilder<PointOfSaleDistributor> builder)
    {
        builder.ToTable("point_of_sale_distributor");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.PointOfSaleId)
            .HasColumnName("point_of_sale_id")
            .IsRequired();

        builder.Property(p => p.DistributorId)
            .HasColumnName("distributor_id")
            .IsRequired();

        builder.Property(p => p.Category)
            .HasColumnName("category")
            .HasConversion<string>() 
            .IsRequired();

        builder.HasIndex(p => new { p.PointOfSaleId, p.DistributorId, p.Category })
               .IsUnique()
               .HasDatabaseName("IX_Pos_Distributor_Category");

        // Optional: configure relationships if needed
        // builder.HasOne<...>()
        //     .WithMany(...)
        //     .HasForeignKey("DistributorId");

        builder
            .HasOne(p => p.Distributor)
            .WithMany(d => d.PointSales)
            .HasForeignKey(p => p.DistributorId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasOne(p => p.PointOfSale)
            .WithMany(p => p.Distributors)
            .HasForeignKey(p => p.PointOfSaleId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}