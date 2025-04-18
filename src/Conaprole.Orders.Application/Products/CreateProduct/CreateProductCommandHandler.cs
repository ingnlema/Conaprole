using Conaprole.Orders.Application.Abstractions.Messaging;
using Conaprole.Orders.Application.Abstractions.Clock;
using Conaprole.Orders.Domain.Abstractions;
using Conaprole.Orders.Domain.Products;
using Conaprole.Orders.Domain.Shared;

namespace Conaprole.Orders.Application.Products.CreateProduct;

internal sealed class CreateProductCommandHandler 
    : ICommandHandler<CreateProductCommand, Guid>
{
    private readonly IProductRepository _productRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDateTimeProvider _dateTimeProvider;

    public CreateProductCommandHandler(
        IProductRepository productRepository,
        IUnitOfWork unitOfWork,
        IDateTimeProvider dateTimeProvider)
    {
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<Result<Guid>> Handle(
        CreateProductCommand request,
        CancellationToken cancellationToken)
    {
        
        var externalProductId = new ExternalProductId(request.ExternalProductId);
        
        var existingProduct = await _productRepository
            .GetByExternalIdAsync(externalProductId, cancellationToken);

        if (existingProduct is not null)
        {
            return Result.Failure<Guid>(ProductErrors.DuplicatedExternalId);
        }
        
        var name = new Name(request.Name);
        var currency = Currency.FromCode(request.CurrencyCode);
        var unitPrice = new Money(request.UnitPrice, currency);
        var description = new Description(request.Description);
        
        var productId = Guid.NewGuid();
        var product = new Product(
            productId,
            externalProductId,
            name,
            unitPrice,
            description,
            _dateTimeProvider.UtcNow
        );
        
        if (request.Categories is not null)
        {
            foreach (var cat in request.Categories)
            {
                product.Categories.Add(new Category(cat));
            }
        }
        
        _productRepository.Add(product);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return product.Id;
    }
}
