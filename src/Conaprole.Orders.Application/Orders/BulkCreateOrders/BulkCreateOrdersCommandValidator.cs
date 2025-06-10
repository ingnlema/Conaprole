using FluentValidation;
using Conaprole.Orders.Application.Orders.CreateOrder;

namespace Conaprole.Orders.Application.Orders.BulkCreateOrders;

public class BulkCreateOrdersCommandValidator : AbstractValidator<BulkCreateOrdersCommand>
{
    public BulkCreateOrdersCommandValidator()
    {
        RuleFor(c => c.Orders)
            .NotEmpty().WithMessage("At least one order is required.")
            .Must(orders => orders.Count <= 100).WithMessage("Cannot create more than 100 orders at once.");

        RuleForEach(c => c.Orders).SetValidator(new CreateOrderCommandValidator());
    }
}