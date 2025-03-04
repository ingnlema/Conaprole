using Conaprole.Orders.Application.Abstractions.Clock;

namespace Conaprole.Orders.Infrastructure.Clock;

internal sealed class DateTimeProvider : IDateTimeProvider
{
    public DateTime UtcNow => DateTime.UtcNow;
}