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
            // Handle Role entities that might be detached but should be marked as existing
            foreach (var entry in ChangeTracker.Entries<Role>())
            {
                if (entry.State == EntityState.Added)
                {
                    // If this is a static role (has an ID), mark it as unchanged instead of added
                    if (entry.Entity.Id > 0)
                    {
                        entry.State = EntityState.Unchanged;
                    }
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