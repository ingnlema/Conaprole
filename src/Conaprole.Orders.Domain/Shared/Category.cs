using System;

namespace Conaprole.Orders.Domain.Shared;

public enum Category
{
    CONGELADOS = 1,
    LACTEOS = 2,
    SUBPRODUCTOS = 3,
    [Obsolete("BEBIDAS category is deprecated. Use another category.")]
    BEBIDAS = 4
}