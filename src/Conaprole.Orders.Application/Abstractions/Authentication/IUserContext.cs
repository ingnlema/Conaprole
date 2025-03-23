namespace Conaprole.Orders.Application.Abstractions.Authentication;

public interface IUserContext
{
    Guid UserId { get; }

    string IdentityId { get; }
}