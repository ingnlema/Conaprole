using FluentValidation;

namespace Conaprole.Orders.Application.Orders.UpdateOrderLineQuantity;

public class UpdateOrderLineQuantityCommandValidator 
    : AbstractValidator<UpdateOrderLineQuantityCommand>
{
    public UpdateOrderLineQuantityCommandValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty().WithMessage("OrderId is required.");
            
        RuleFor(x => x.ProductId)
            .NotEmpty().WithMessage("ProductId is required.");
            
        RuleFor(x => x.NewQuantity)
            .GreaterThan(0).WithMessage("NewQuantity must be greater than zero.");
    }
}