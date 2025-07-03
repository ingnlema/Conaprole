using System.Text.Json.Serialization;
using Conaprole.Orders.Api.Controllers.Orders.Examples;
using Conaprole.Orders.Api.Extensions;
using Conaprole.Orders.Application;
using Conaprole.Orders.Infrastructure;
using Conaprole.Orders.Infrastructure.Authentication;
using Microsoft.OpenApi.Models;
using Serilog;
using Swashbuckle.AspNetCore.Filters;

// Configure Serilog before creating the builder
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day)
    .MinimumLevel.Information()
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

// Use Serilog as the logging provider
builder.Host.UseSerilog();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerExamplesFromAssemblyOf<UpdateOrderStatusRequestExample>();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Conaprole Orders API",
        Version = "v1.1"
    });

    options.CustomSchemaIds(type => type.FullName); 

    options.EnableAnnotations();

    // Esquema de seguridad JWT
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: 'Bearer {token}'",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddApplication();
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

// Swagger debe ir antes para asegurar accesibilidad
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Conaprole Orders API v1.1");
    c.RoutePrefix = "swagger";
});

// Aplicar migraciones según config
var applyMigrations = builder.Configuration.GetValue<bool>("APPLY_MIGRATIONS");
if (applyMigrations || app.Environment.IsDevelopment())
{
    //app.UseDeveloperExceptionPage();
    app.ApplyMigrations();
    // app.SeedData(); // habilitá si querés precargar datos
}

// Crear usuario administrador inicial solo en Development y Staging
if (app.Environment.IsDevelopment() || app.Environment.IsStaging())
{
    Log.Information("Environment is {Environment}. Creating initial admin user...", app.Environment.EnvironmentName);
    try
    {
        using var scope = app.Services.CreateScope();
        var initialAdminUserService = scope.ServiceProvider.GetRequiredService<IInitialAdminUserService>();
        await initialAdminUserService.CreateInitialAdminUserAsync();
        Log.Information("Initial admin user creation process completed.");
    }
    catch (Exception ex)
    {
        Log.Error(ex, "Failed to create initial admin user during startup.");
    }
}
else
{
    Log.Information("Environment is {Environment}. Skipping initial admin user creation.", app.Environment.EnvironmentName);
}

app.UseCors();
app.UseHttpsRedirection();
app.UseCustomExceptionHandler();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

try
{
    Log.Information("Starting the Conaprole Orders API");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

public partial class Program;