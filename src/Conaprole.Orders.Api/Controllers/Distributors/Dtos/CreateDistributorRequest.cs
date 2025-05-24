namespace Conaprole.Orders.Api.Controllers.Distributors.Dtos;

public record CreateDistributorRequest(
    string Name,
    string PhoneNumber,
    string Address,
    List<string> Categories
);