using System.Data;

namespace Conaprole.Orders.Application.Abstractions.Data;

public interface ISqlConnectionFactory
{
    IDbConnection CreateConnection();
}