using Conaprole.Orders.Domain.Shared;
using Microsoft.AspNetCore.Mvc;

namespace Conaprole.Orders.Api.Controllers;

public static class CategoryHelper
{
    public static ActionResult<Category>? TryParseCategory(string categoryString, ControllerBase controller)
    {
        if (!Enum.TryParse<Category>(categoryString, true, out var category))
        {
            return controller.BadRequest($"Invalid category: {categoryString}");
        }

        // Check for deprecated category
        if (category == Category.BEBIDAS)
        {
            return controller.BadRequest("BEBIDAS category is deprecated. Please use another category.");
        }

        return category;
    }

    public static ActionResult<List<Category>>? TryParseCategories(List<string> categoryStrings, ControllerBase controller)
    {
        var categories = new List<Category>();
        
        foreach (var categoryString in categoryStrings)
        {
            if (!Enum.TryParse<Category>(categoryString, true, out var category))
            {
                return controller.BadRequest($"Invalid category: {categoryString}");
            }

            // Check for deprecated category
            if (category == Category.BEBIDAS)
            {
                return controller.BadRequest("BEBIDAS category is deprecated. Please use another category.");
            }

            categories.Add(category);
        }

        return categories;
    }
}