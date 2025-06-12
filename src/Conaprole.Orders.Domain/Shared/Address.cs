using System.Text.RegularExpressions;

namespace Conaprole.Orders.Domain.Shared;

public record Address(
    string City,
    string Street,
    string ZipCode
    )
{
    /// <summary>
    /// Parses an Address from its ToString() representation.
    /// Expected format: "Address { City = <city>, Street = <street>, ZipCode = <zipcode> }"
    /// </summary>
    public static Address FromString(string addressString)
    {
        if (string.IsNullOrWhiteSpace(addressString))
        {
            return new Address("", "", "");
        }

        // Pattern to match: Address { City = <value>, Street = <value>, ZipCode = <value> }
        var pattern = @"Address\s*\{\s*City\s*=\s*([^,]+),\s*Street\s*=\s*([^,]+),\s*ZipCode\s*=\s*([^}]+)\s*\}";
        var match = Regex.Match(addressString, pattern);
        
        if (match.Success)
        {
            var city = match.Groups[1].Value.Trim();
            var street = match.Groups[2].Value.Trim();
            var zipCode = match.Groups[3].Value.Trim();
            
            return new Address(city, street, zipCode);
        }
        
        // Fallback: if parsing fails, return empty address
        return new Address("", "", "");
    }
};