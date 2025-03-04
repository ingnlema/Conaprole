namespace Conaprole.Orders.Application.Orders.AddOrderLine;

using FluentValidation;

public class AddOrderLineToOrderCommandValidator : AbstractValidator<AddOrderLineToOrderCommand>
{
    public AddOrderLineToOrderCommandValidator()
    {
        RuleFor(c => c.OrderId).NotEmpty();
        RuleFor(c => c.ProductId).NotEmpty();
        RuleFor(c => c.Quantity).GreaterThan(0);
        RuleFor(c => c.UnitPrice).GreaterThan(0);
        RuleFor(c => c.CurrencyCode).NotEmpty();
    }
}
