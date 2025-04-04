
using FluentValidation;

namespace Conaprole.Orders.Application.Orders.CreateOrder;

public class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderCommandValidator()
    {
        RuleFor(c => c.PointOfSalePhoneNumber)
            .NotEmpty().WithMessage("Phone number is required.")
            .MaximumLength(20);
        RuleFor(c => c.Distributor).NotEmpty();
        RuleFor(c => c.City).NotEmpty();
        RuleFor(c => c.Street).NotEmpty();
        RuleFor(c => c.ZipCode).NotEmpty();
        RuleFor(c => c.CurrencyCode).NotEmpty();

        RuleFor(c => c.OrderLines)
            .NotEmpty().WithMessage("At least one order line is required.");

        RuleForEach(c => c.OrderLines).SetValidator(new CreateOrderLineCommandValidator());
    }
}

public class CreateOrderLineCommandValidator : AbstractValidator<CreateOrderLineCommand>
{
    public CreateOrderLineCommandValidator()
    {
        RuleFor(ol => ol.ExternalProductId).NotEmpty();
        RuleFor(ol => ol.Quantity).GreaterThan(0);
    }
}
