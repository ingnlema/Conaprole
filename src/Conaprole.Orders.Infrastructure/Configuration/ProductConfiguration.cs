using Conaprole.Orders.Domain.Products;
using Conaprole.Orders.Domain.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Conaprole.Orders.Infrastructure.Configuration;

public sealed class ProductConfiguration : IEntityTypeConfiguration<Product>
{
     public void Configure(EntityTypeBuilder<Product> builder)
        {

            builder.ToTable("products");
            builder.HasKey(p => p.Id);


            builder.Property(p => p.ExternalProductId)
                .HasConversion(
                    ep => ep.Value,
                    value => new ExternalProductId(value))
                .HasColumnName("external_product_id")
                .IsRequired();
            
            builder.Property(p => p.Name)
                .HasConversion(
                    n => n.Value,
                    value => new Name(value))
                .HasColumnName("name")
                .IsRequired();
            
            builder.Property(p => p.Description)
                .HasConversion(
                    d => d.Value,
                    value => new Description(value))
                .HasColumnName("description")
                .IsRequired();
            
            builder.OwnsOne(p => p.UnitPrice, money =>
            {
                money.Property(m => m.Amount)
                     .HasColumnName("unit_price_amount")
                     .HasColumnType("decimal(18,2)")
                     .IsRequired();
                
                money.Property(m => m.Currency)
                     .HasColumnName("unit_price_currency")
                     .HasConversion(
                         c => c.Code,
                         code => Currency.FromCode(code))
                     .IsRequired();
            });
            
            builder.Property(p => p.LastUpdated)
                .HasColumnName("last_updated")
                .IsRequired();
            
            builder.Property(p => p.Category)
                .HasConversion(
                    c => (int)c,
                    value => (Category)value)
                .HasColumnName("category")
                .IsRequired();
        }
}