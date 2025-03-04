using Conaprole.Orders.Domain.Orders;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Conaprole.Orders.Infrastructure.Configuration;

public sealed class OrderLineConfiguration : IEntityTypeConfiguration<OrderLine>
{
    public void Configure(EntityTypeBuilder<OrderLine> builder)
    {
        builder.ToTable("order_lines");
        builder.HasKey(orderLine => orderLine.Id);

    }
}