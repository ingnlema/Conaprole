// File: Conaprole.Orders.Application/Orders/RemoveOrderLine/RemoveOrderLineFromOrderCommandValidator.cs
using FluentValidation;

namespace Conaprole.Orders.Application.Orders.RemoveOrderLine
{
    public class RemoveOrderLineFromOrderCommandValidator
        : AbstractValidator<RemoveOrderLineFromOrderCommand>
    {
        public RemoveOrderLineFromOrderCommandValidator()
        {
            RuleFor(x => x.OrderId)
                .NotEmpty().WithMessage("OrderId is required.");

            RuleFor(x => x.OrderLineId)
                .NotEmpty().WithMessage("OrderLineId is required.");
        }
    }
}