using Conaprole.Orders.Api.Middelware;
using Conaprole.Orders.Infrastructure;
using Microsoft.AspNetCore.Diagnostics;

namespace Conaprole.Orders.Api.Extensions;

using Microsoft.EntityFrameworkCore;

public static class ApplicationBuilderExtensions
{
    public static void ApplyMigrations(this IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices.CreateScope();

        using var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        dbContext.Database.Migrate();
    }

    public static void UseCustomExceptionHandler(this IApplicationBuilder app)
    {
        app.UseMiddleware<ExceptionHandlingMiddleware>();
    }


}
