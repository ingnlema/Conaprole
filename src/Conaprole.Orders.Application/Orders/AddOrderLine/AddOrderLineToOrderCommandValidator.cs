// Conaprole.Orders.Application/Orders/AddOrderLine/AddOrderLineToOrderCommandValidator.cs
using System;
using FluentValidation;
using Conaprole.Orders.Application.Orders.AddOrderLine;

namespace Conaprole.Orders.Application.Orders.AddOrderLine
{
    public class AddOrderLineToOrderCommandValidator : AbstractValidator<AddOrderLineToOrderCommand>
    {
        public AddOrderLineToOrderCommandValidator()
        {
            RuleFor(x => x.OrderId)
                .NotEmpty().WithMessage("OrderId is required.");

            RuleFor(x => x.ExternalProductId)
                .NotNull().WithMessage("ExternalProductId is required.")
                .Must(ep => !string.IsNullOrWhiteSpace(ep.Value))
                .WithMessage("ExternalProductId is required.");

            RuleFor(x => x.Quantity)
                .GreaterThan(0).WithMessage("Quantity must be greater than zero.");
        }
    }
}