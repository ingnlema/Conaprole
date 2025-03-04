using FluentValidation;

namespace Conaprole.Orders.Application.Orders.CreateOrder;

public class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderCommandValidator()
    {
        RuleFor(c => c.PointOfSaleId).NotEmpty();
        RuleFor(c => c.Distributor).NotEmpty();
        RuleFor(c => c.City).NotEmpty();
        RuleFor(c => c.Street).NotEmpty();
        RuleFor(c => c.ZipCode).NotEmpty();
        RuleFor(c => c.Price).GreaterThan(0);
        RuleFor(c => c.CurrencyCode).NotEmpty();
    }
}