namespace Conaprole.Orders.Application.Orders.AddOrderLine;

using FluentValidation;

public class AddOrderLineToOrderCommandValidator : AbstractValidator<AddOrderLineToOrderCommand>
{
    public AddOrderLineToOrderCommandValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty().WithMessage("OrderId is required.");
            
        RuleFor(x => x.ProductId)
            .NotEmpty().WithMessage("ProductId is required.");
            
        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("Quantity must be greater than zero.");
            
        RuleFor(x => x.UnitPrice)
            .GreaterThan(0m).WithMessage("UnitPrice must be greater than zero.");
            
        RuleFor(x => x.CurrencyCode)
            .NotEmpty().WithMessage("CurrencyCode is required.");
    }
}

