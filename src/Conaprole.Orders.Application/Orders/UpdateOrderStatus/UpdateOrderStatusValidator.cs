
using FluentValidation;
using Conaprole.Orders.Domain.Orders;

namespace Conaprole.Orders.Application.Orders.UpdateOrderStatus
{
    public class UpdateOrderStatusCommandValidator : AbstractValidator<UpdateOrderStatusCommand>
    {
        public UpdateOrderStatusCommandValidator()
        {
            RuleFor(cmd => cmd.OrderId)
                .NotEqual(Guid.Empty)
                .WithMessage("Order ID is required.");

            RuleFor(cmd => cmd.NewStatus)
                .NotEqual(Status.Created)
                .WithMessage("Order status cannot be updated to 'Created'.");
        }
    }
}