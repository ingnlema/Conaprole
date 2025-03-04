namespace Conaprole.Orders.Domain.Orders;

public enum Status
{
    Created = 0,
    Confirmed = 1,
    Delivered = 2,
    Canceled = -2,
    Rejected = -1
}