
namespace Conaprole.Orders.Api.Extensions;

using Application.Abstractions.Data;
using Bogus;
using Dapper;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;


public static class SeedDataExtensions
{
    public static void SeedData(this IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices.CreateScope();
        var sqlConnectionFactory = scope.ServiceProvider.GetRequiredService<ISqlConnectionFactory>();
        using var connection = sqlConnectionFactory.CreateConnection();
        
        var faker = new Faker("es");
        
        var products = new List<dynamic>();
        for (int i = 0; i < 50; i++)
        {
            products.Add(new
            {
                Id = Guid.NewGuid(),
                ExternalProductId = faker.Random.AlphaNumeric(10),
                Name = faker.Commerce.ProductName(), 
                UnitPriceAmount = faker.Random.Decimal(50, 1500), 
                UnitPriceCurrency = "UYU",
                Description = faker.Commerce.ProductDescription(),
                LastUpdated = DateTime.UtcNow
            });
        }

        const string sqlProducts = @"
            INSERT INTO products 
                (id, external_product_id, name, unit_price_amount, unit_price_currency, description, last_updated)
            VALUES 
                (@Id, @ExternalProductId, @Name, @UnitPriceAmount, @UnitPriceCurrency, @Description, @LastUpdated);
        ";

        connection.Execute(sqlProducts, products);
        
        var availableCategories = new[] { "Congelados", "Lácteos", "Carnes", "Bebidas", "Panificados" };

        var productCategories = new List<dynamic>();
        
        foreach (var product in products)
        {
            var count = faker.Random.Int(1, 2);
            var randomCategories = faker.PickRandom(availableCategories, count);
            foreach (var category in randomCategories)
            {
                productCategories.Add(new
                {
                    product_id = product.Id,
                    category = category
                });
            }
        }

        const string sqlCategories = @"
            INSERT INTO product_categories 
                (product_id, category)
            VALUES 
                (@product_id, @category);
        ";

        connection.Execute(sqlCategories, productCategories);
    }
}

