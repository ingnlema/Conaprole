using Conaprole.Orders.Api.Controllers.Orders.Examples;
using Conaprole.Orders.Api.Extensions;
using Conaprole.Orders.Application;
using Conaprole.Orders.Infrastructure;
using Swashbuckle.AspNetCore.Filters;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerExamplesFromAssemblyOf<UpdateOrderStatusRequestExample>();
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
        c.ExampleFilters();
    }
    catch (Exception ex)
    {
        Console.WriteLine("Swagger config failed: " + ex.Message);
    }
});


builder.Services.AddApplication();
Console.WriteLine("DB: " + builder.Configuration.GetConnectionString("Database"));
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});


var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Conaprole Orders API v1");
});

var applyMigrations = builder.Configuration.GetValue<bool>("APPLY_MIGRATIONS");
if (applyMigrations)
{
    app.ApplyMigrations();
}


if (app.Environment.IsDevelopment())
{
    // REMARK: Uncomment if you want to seed initial data.
    app.ApplyMigrations();
    //app.SeedData();
}

app.UseCors();

app.UseHttpsRedirection();

app.UseCustomExceptionHandler();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program;

