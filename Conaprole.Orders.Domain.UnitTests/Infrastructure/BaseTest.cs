using Conaprole.Orders.Domain.Abstractions;

namespace Conaprole.Orders.Domain.UnitTests.Infrastructure;

public abstract class BaseTest
{
   public static T AssertDomainEventWasPublished<T>(Entity entity)
      where T : IDomainEvent
   {
      var domainEvent = entity.GetDomainEvents().OfType<T>().SingleOrDefault();
      if (domainEvent == null)
         throw new Exception($"Domain event of type {typeof(T).Name} was not published.");
      return domainEvent;
   }
}