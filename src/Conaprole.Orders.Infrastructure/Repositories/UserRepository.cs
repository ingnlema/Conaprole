using Conaprole.Orders.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Conaprole.Orders.Infrastructure.Repositories;

internal sealed class UserRepository : Repository<User>, IUserRepository
{
    public UserRepository(ApplicationDbContext dbContext)
        : base(dbContext)
    {
    }

    public override async Task<User?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await DbContext
            .Set<User>()
            .Include(u => u.Roles)
            .FirstOrDefaultAsync(entity => entity.Id == id, cancellationToken);
    }

    public async Task<User?> GetByEmailAsync(
        string email,
        CancellationToken cancellationToken = default)
    {
        return await DbContext
            .Set<User>()
            .Include(u => u.Roles)
            .FirstOrDefaultAsync(u => EF.Property<string>(u, "Email") == email, cancellationToken);
    }

    public override void Add(User user)
    {
        foreach (var role in user.Roles)
        {
            DbContext.Attach(role);
        }

        DbContext.Add(user);
    }
}