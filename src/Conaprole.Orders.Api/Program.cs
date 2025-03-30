using Conaprole.Orders.Api.Extensions;
using Conaprole.Orders.Application;
using Conaprole.Orders.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    try
    {
        c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
        {
            Title = "Conaprole Orders API",
            Version = "v1"
        });

        c.EnableAnnotations();
    }
    catch (Exception ex)
    {
        Console.WriteLine("Swagger config failed: " + ex.Message);
    }
});


builder.Services.AddApplication();
Console.WriteLine("DB: " + builder.Configuration.GetConnectionString("Database"));
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Conaprole Orders API v1");
});

var applyMigrations = builder.Configuration.GetValue<bool>("ApplyMigrations");
if (applyMigrations)
{
    app.ApplyMigrations();
}


if (app.Environment.IsDevelopment())
{
    // REMARK: Uncomment if you want to seed initial data.
    //app.SeedData();
}

app.UseHttpsRedirection();

app.UseCustomExceptionHandler();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
