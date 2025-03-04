using Conaprole.Orders.Application.Abstractions.Clock;
using Conaprole.Orders.Application.Abstractions.Data;
using Conaprole.Orders.Domain.Abstractions;
using Conaprole.Orders.Domain.Orders;
using Conaprole.Orders.Domain.Products;
using Conaprole.Orders.Infrastructure.Clock;
using Conaprole.Orders.Infrastructure.Data;
using Conaprole.Orders.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Conaprole.Orders.Infrastructure;


using Dapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;


public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddTransient<IDateTimeProvider, DateTimeProvider>();
        
        var connectionString =
            configuration.GetConnectionString("Database") ??
            throw new ArgumentNullException(nameof(configuration));

        services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseNpgsql(connectionString).UseSnakeCaseNamingConvention();
        });

        
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IProductRepository, ProductRepository>();

        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<ApplicationDbContext>());

        services.AddSingleton<ISqlConnectionFactory>(_ =>
            new SqlConnectionFactory(connectionString));

        SqlMapper.AddTypeHandler(new DateOnlyTypeHandler());
        return services;
    }
}