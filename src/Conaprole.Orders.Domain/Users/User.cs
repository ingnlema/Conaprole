using Conaprole.Orders.Domain.Abstractions;
using Conaprole.Orders.Domain.Distributors;
using Conaprole.Orders.Domain.Users.Events;

namespace Conaprole.Orders.Domain.Users;

public sealed class User : Entity
{
    private readonly List<Role> _roles = new();

    private User(Guid id, FirstName firstName, LastName lastName, Email email)
        : base(id)
    {
        FirstName = firstName;
        LastName = lastName;
        Email = email;
    }

    private User()
    {
    }

    public FirstName FirstName { get; private set; }

    public LastName LastName { get; private set; }

    public Email Email { get; private set; }

    public string IdentityId { get; private set; } = string.Empty;

    public Guid? DistributorId { get; private set; }

    public Distributor? Distributor { get; private set; }

    // EF Core needs access to the actual collection for change tracking
    public ICollection<Role> Roles => _roles;

    public static User Create(FirstName firstName, LastName lastName, Email email)
    {
        var user = new User(Guid.NewGuid(), firstName, lastName, email);

        user.RaiseDomainEvent(new UserCreatedDomainEvent(user.Id));

        return user;
    }

    public void SetIdentityId(string identityId)
    {
        IdentityId = identityId;
    }

    public void SetDistributor(Guid distributorId)
    {
        DistributorId = distributorId;
    }

    public void AssignRole(Role role)
    {
        if (_roles.Any(r => r.Id == role.Id))
        {
            return; // Already has the role
        }
        
        _roles.Add(role);
        RaiseDomainEvent(new UserRoleAssignedDomainEvent(Id, role.Id));
    }

    public void RemoveRole(Role role)
    {
        var existingRole = _roles.FirstOrDefault(r => r.Id == role.Id);
        if (existingRole != null)
        {
            _roles.Remove(existingRole);
            RaiseDomainEvent(new UserRoleRemovedDomainEvent(Id, role.Id));
        }
    }
}