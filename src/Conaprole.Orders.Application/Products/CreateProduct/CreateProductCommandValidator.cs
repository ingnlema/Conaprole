using FluentValidation;

namespace Conaprole.Orders.Application.Products.CreateProduct;

public class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator()
    {
        RuleFor(c => c.ExternalProductId).NotEmpty();
        RuleFor(c => c.Name).NotEmpty();
        RuleFor(c => c.UnitPrice).GreaterThanOrEqualTo(0);
        RuleFor(c => c.CurrencyCode).NotEmpty();
        RuleFor(c => c.Description).NotEmpty();
    }
}