using Conaprole.Orders.Domain.Exceptions;

namespace Conaprole.Orders.Domain.Shared;

public record Quantity
{
    public int Value { get; }

    public Quantity(int value)
    {
        if (value <= 0)
            throw new DomainException("Quantity must be greater than zero.");
        Value = value;
    }

    public static implicit operator int(Quantity q) => q.Value;
    public static explicit operator Quantity(int value) => new Quantity(value);
}