# Fix for Role Entity Tracking Issue

## Problem
The original issue was that when running multiple role assignment tests together, an `InvalidOperationException` was thrown with the message:

> The instance of entity type 'Role' cannot be tracked because another instance with the same key value for {'Id'} is already being tracked.

This occurred because the `AssignRoleCommandHandler` and `RemoveRoleCommandHandler` were using static `Role` instances (`Role.Administrator`, `Role.Registered`, etc.) directly from the domain model. When these static instances were used across multiple tests in the same test run, EF Core detected that the same Role entity (with the same Id) was already being tracked from a previous test, causing the tracking conflict.

## Solution
The solution was to modify the command handlers to retrieve roles from the database instead of using static domain instances:

1. **Created IRoleRepository interface** - A simple repository interface with a `GetByNameAsync` method
2. **Implemented RoleRepository** - Uses EF Core to query roles from the database by name
3. **Modified AssignRoleCommandHandler** - Now uses IRoleRepository instead of static Role instances
4. **Modified RemoveRoleCommandHandler** - Now uses IRoleRepository instead of static Role instances  
5. **Updated DI configuration** - Registered IRoleRepository in the dependency injection container

## Key Changes

### IRoleRepository Interface
```csharp
public interface IRoleRepository
{
    Task<Role?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
}
```

### Command Handlers
Instead of:
```csharp
var role = GetRoleByName(request.RoleName); // Static method returning static instances
```

Now:
```csharp
var role = await _roleRepository.GetByNameAsync(request.RoleName, cancellationToken);
```

## Benefits
- **Eliminates EF Core tracking conflicts** - Roles are now properly tracked entities from the database context
- **Maintains consistency** - Roles are always the same instance within a given DbContext scope
- **Preserves existing behavior** - The logic for role assignment/removal remains unchanged
- **Clean architecture** - Uses repository pattern consistently with other entities

## Testing
All role assignment and removal tests now pass when run together, including the previously failing `AssignRole_ShouldAssignRoleToUser_WhenValidRequest` test.