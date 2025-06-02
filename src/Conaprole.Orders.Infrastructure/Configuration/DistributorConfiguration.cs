using Conaprole.Orders.Domain.Distributors;
using Conaprole.Orders.Domain.PointsOfSale;
using Conaprole.Orders.Domain.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Conaprole.Orders.Infrastructure.Configuration;

public sealed class DistributorConfiguration : IEntityTypeConfiguration<Distributor>
{
    public void Configure(EntityTypeBuilder<Distributor> builder)
    {
        builder.ToTable("distributor");
        builder.HasKey(d => d.Id);

        builder.Property(d => d.Id)
               .HasColumnName("id");

        builder.Property(d => d.PhoneNumber)
               .HasColumnName("phone_number")
               .IsRequired()
               .HasMaxLength(20);

        builder.Property(d => d.Name)
               .HasColumnName("name")
               .IsRequired()
               .HasMaxLength(100);

        builder.Property(d => d.Address)
               .HasColumnName("address")
               .IsRequired()
               .HasMaxLength(200);

        builder.Property(d => d.CreatedAt)
               .HasColumnName("created_at")
               .IsRequired();

        builder.Property(d => d.SupportedCategories)
               .HasColumnName("supported_categories")
               .HasConversion(
                   v => string.Join(',', v.Select(c => c.ToString())),
                   v => string.IsNullOrEmpty(v) 
                       ? new List<Category>() 
                       : v.Split(',', StringSplitOptions.RemoveEmptyEntries)
                         .Select(s => Enum.Parse<Category>(s, true))
                         .ToList()
               )
               .Metadata.SetValueComparer(
                   new ValueComparer<ICollection<Category>>(
                       (c1, c2) => c1.SequenceEqual(c2),
                       c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                       c => c.ToList()
                   )
               );

        builder.HasMany(d => d.PointSales)
               .WithOne()
               .HasForeignKey(ps => ps.DistributorId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}