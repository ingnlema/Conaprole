namespace Conaprole.Orders.Domain.Shared;

public record  Money(decimal Amount, Currency Currency)
{
    public static Money operator +(Money first, Money second)
    {
        if (first.Currency != second.Currency)
        {
            throw new InvalidOperationException("Currencies have to be equal");
        }

        return new Money(first.Amount + second.Amount, first.Currency);
    }
    
    public static Money operator *(Money money, Quantity quantity)
    {
        return new Money(money.Amount * quantity.Value, money.Currency);
    }
    
    public static Money operator *(Quantity quantity, Money money)
    {
        return money * quantity;
    }
    
    public static Money operator -(Money first, Money second)
    {
        if (first.Currency != second.Currency)
        {
            throw new InvalidOperationException("Currencies have to be equal");
        }
        
        if (first.Amount < second.Amount)
        {
            throw new InvalidOperationException("Invalid Money operation");
        }

        return new Money(first.Amount - second.Amount, first.Currency);
    }



    public static Money Zero() => new(0, Currency.None);

    public static Money Zero(Currency currency) => new(0, currency);

    public bool IsZero() => this == Zero(Currency);
}