using FluentValidation;

namespace Conaprole.Orders.Application.Orders.RemoveOrderLine;

public class RemoveOrderLineCommandValidator 
    : AbstractValidator<RemoveOrderLineCommand>
{
    public RemoveOrderLineCommandValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty().WithMessage("OrderId is required.");
            
        RuleFor(x => x.ProductId)
            .NotEmpty().WithMessage("ProductId is required.");
    }
}