using Conaprole.Orders.Application.Orders.CreateOrder;
using MediatR;

namespace Conaprole.Orders.Application.IntegrationTests.Orders
{
    /// <summary>
    /// Encapsula datos y creación de una orden para tests de integración.
    /// </summary>
    public static class OrderData
    {
        public const string City = "Montevideo";
        public const string Street = "Calle Test 789";
        public const string ZipCode = "54321";
        public const string CurrencyCode = "UYU";

        /// <summary>
        /// Crea una orden con las líneas especificadas vía MediatR y devuelve su ID.
        /// </summary>
        public static async Task<Guid> SeedAsync(
            ISender sender,
            string pointOfSalePhoneNumber,
            string distributorPhoneNumber,
            List<CreateOrderLineCommand> orderLines)
        {
            var command = new CreateOrderCommand(
                pointOfSalePhoneNumber,
                distributorPhoneNumber,
                City,
                Street,
                ZipCode,
                CurrencyCode,
                orderLines
            );

            var result = await sender.Send(command);
            if (result.IsFailure)
                throw new Exception($"Error seeding order: {result.Error.Code}");
            return result.Value;
        }

        /// <summary>
        /// Crea una línea de orden con un producto específico.
        /// </summary>
        public static CreateOrderLineCommand CreateOrderLine(string externalProductId, int quantity = 1) =>
            new(externalProductId, quantity);
    }
}