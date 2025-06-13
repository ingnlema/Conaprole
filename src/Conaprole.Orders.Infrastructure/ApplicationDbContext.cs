using Conaprole.Orders.Application.Exceptions;
using Conaprole.Orders.Domain.Abstractions;
using Conaprole.Orders.Domain.Products;
using Conaprole.Orders.Domain.Users;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Conaprole.Orders.Infrastructure;

public sealed class ApplicationDbContext : DbContext, IUnitOfWork
{
    
    private readonly IPublisher _publisher;
    public DbSet<Product> Products { get; set; } = null!;
    public DbSet<Role> Roles { get; set; } = null!;


    public ApplicationDbContext(DbContextOptions options, IPublisher publisher)
        : base(options)
    {
        _publisher = publisher;
    }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }
    
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            // Manually handle role tracking to prevent conflicts with static role instances
            var trackedRoles = new Dictionary<int, Role>();
            
            // First pass: identify all roles being tracked and collect unique instances
            var entries = ChangeTracker.Entries().ToList();
            foreach (var entry in entries)
            {
                if (entry.Entity is Role role && role.Id > 0)
                {
                    if (!trackedRoles.ContainsKey(role.Id))
                    {
                        trackedRoles[role.Id] = role;
                    }
                }
            }
            
            // Second pass: replace role references in users with the tracked instances
            foreach (var entry in entries)
            {
                if (entry.Entity is User user)
                {
                    var rolesToReplace = new List<Role>();
                    var rolesToAdd = new List<Role>();
                    
                    foreach (var userRole in user.Roles.ToList())
                    {
                        if (userRole.Id > 0 && trackedRoles.TryGetValue(userRole.Id, out var trackedRole))
                        {
                            if (userRole != trackedRole)
                            {
                                rolesToReplace.Add(userRole);
                                rolesToAdd.Add(trackedRole);
                            }
                        }
                    }
                    
                    // Replace the roles
                    foreach (var roleToReplace in rolesToReplace)
                    {
                        user.Roles.Remove(roleToReplace);
                    }
                    foreach (var roleToAdd in rolesToAdd)
                    {
                        if (!user.Roles.Contains(roleToAdd))
                        {
                            user.Roles.Add(roleToAdd);
                        }
                    }
                }
            }
            
            // Mark static roles as unchanged to prevent EF from trying to insert them
            foreach (var entry in ChangeTracker.Entries<Role>())
            {
                if (entry.Entity.Id > 0 && entry.State == EntityState.Added)
                {
                    entry.State = EntityState.Unchanged;
                }
            }

            var result = await base.SaveChangesAsync(cancellationToken);
            
            await PublishDomainEventsAsync();

            return result;
        }
        catch (DbUpdateConcurrencyException ex)
        {
            throw new ConcurrencyException("Concurrency exception occurred.", ex);
        }
    }

    private async Task PublishDomainEventsAsync()
    {
        var domainEvents = ChangeTracker
            .Entries<Entity>()
            .Select(entry => entry.Entity)
            .SelectMany(entity =>
            {
                var domainEvents = entity.GetDomainEvents();

                entity.ClearDomainEvents();

                return domainEvents;
            })
            .ToList();

        foreach (var domainEvent in domainEvents)
        {
            await _publisher.Publish(domainEvent);
        }
    }
 
}