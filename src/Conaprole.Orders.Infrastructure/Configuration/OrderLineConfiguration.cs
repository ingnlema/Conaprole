using Conaprole.Orders.Domain.Orders;
using Conaprole.Orders.Domain.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Conaprole.Orders.Infrastructure.Configuration;

public sealed class OrderLineConfiguration : IEntityTypeConfiguration<OrderLine>
{
    public void Configure(EntityTypeBuilder<OrderLine> builder)
    {
        builder.ToTable("order_lines");
        builder.HasKey(ol => ol.Id);
        
        builder.Property(ol => ol.Quantity)
            .HasConversion(
                q => q.Value,
                value => new Quantity(value))
            .HasColumnName("quantity")
            .IsRequired();
        
        builder.OwnsOne(ol => ol.SubTotal, money =>
        {
            money.Property(m => m.Amount)
                .HasColumnName("sub_total_amount")
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            money.Property(m => m.Currency)
                .HasColumnName("sub_total_currency")
                .HasConversion(
                    c => c.Code,
                    code => Currency.FromCode(code))
                .IsRequired();
        });
        
        builder.Property(ol => ol.OrderId)
            .HasConversion(
                oid => oid.Value,
                value => new OrderId(value))
            .HasColumnName("order_id")
            .IsRequired();

        builder.Property(ol => ol.CreatedOnUtc)
            .HasColumnName("created_on_utc")
            .IsRequired();


        builder.HasOne(ol => ol.Product)
            .WithMany() 
            .HasForeignKey("product_id")
            .IsRequired();
    }
}