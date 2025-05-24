using Conaprole.Orders.Domain.Orders;
using Conaprole.Orders.Domain.Shared;
using Conaprole.Orders.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Conaprole.Orders.Infrastructure.Configuration;

public sealed class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
        {

            builder.ToTable("orders");
            builder.HasKey(o => o.Id);

            // Distributor and PointOfSale configuration moved to other configuration files.
            builder.Property(o => o.Status)
                .HasColumnName("status")
                .IsRequired();

            builder.Property(o => o.CreatedOnUtc)
                .HasColumnName("created_on_utc")
                .IsRequired();

            builder.Property(o => o.ConfirmedOnUtc)
                .HasColumnName("confirmed_on_utc");

            builder.Property(o => o.RejectedOnUtc)
                .HasColumnName("rejected_on_utc");

            builder.Property(o => o.DeliveryOnUtc)
                .HasColumnName("delivery_on_utc");

            builder.Property(o => o.CanceledOnUtc)
                .HasColumnName("canceled_on_utc");

            builder.Property(o => o.DeliveredOnUtc)
                .HasColumnName("delivered_on_utc");
            
            builder.OwnsOne(o => o.DeliveryAddress, address =>
            {
                address.Property(a => a.City)
                    .HasColumnName("delivery_address_city")
                    .IsRequired();

                address.Property(a => a.Street)
                    .HasColumnName("delivery_address_street")
                    .IsRequired();

                address.Property(a => a.ZipCode)
                    .HasColumnName("delivery_address_zipcode")
                    .IsRequired();
            });


            builder.OwnsOne(o => o.Price, money =>
            {
                money.Property(m => m.Amount)
                     .HasColumnName("price_amount")
                     .HasColumnType("decimal(18,2)")
                     .IsRequired();

                money.Property(m => m.Currency)
                     .HasColumnName("price_currency")
                     .HasConversion(
                         currency => currency.Code,
                         code => Currency.FromCode(code))
                     .IsRequired();
            });
            
            builder.HasMany(o => o.OrderLines)
                   .WithOne() 
                   .HasForeignKey("order_id") 
                   .OnDelete(DeleteBehavior.Cascade);
            
        }
    
}