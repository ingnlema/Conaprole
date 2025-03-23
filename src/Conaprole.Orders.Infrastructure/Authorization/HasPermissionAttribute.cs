using Microsoft.AspNetCore.Authorization;

namespace Conaprole.Orders.Infrastructure.Authorization;

public sealed class HasPermissionAttribute : AuthorizeAttribute
{
    public HasPermissionAttribute(string permission)
        : base(permission)
    {
    }
}