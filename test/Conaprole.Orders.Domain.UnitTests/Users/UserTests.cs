using Conaprole.Orders.Domain.UnitTests.Infrastructure;
using Conaprole.Orders.Domain.Users;
using Conaprole.Orders.Domain.Users.Events;
using FluentAssertions;

namespace Conaprole.Orders.Domain.UnitTests.Users;

public class UserTests : BaseTest
{
    [Fact]
    public void Create_Should_SetPropertyValues()
    {
        //Act
        var user = User.Create(UserData.FirstName,UserData.LastName,UserData.Email);
        //Assert
        user.FirstName.Should().Be(UserData.FirstName);
        user.LastName.Should().Be(UserData.LastName);
        user.Email.Should().Be(UserData.Email);
    }
    [Fact]
    public void Create_Should_RaiseUserCreatedDomainEvent()
    {
        //Act
        var user = User.Create(UserData.FirstName,UserData.LastName,UserData.Email);
        //Assert
        var domainEvent = AssertDomainEventWasPublished<UserCreatedDomainEvent>(user);
        domainEvent.UserId.Should().Be(user.Id);

    }

    [Fact]
    public void Create_Should_NotHaveAnyRolesByDefault()
    {
        //Act
        var user = User.Create(UserData.FirstName,UserData.LastName,UserData.Email);
        //Assert
        user.Roles.Should().BeEmpty();
    }

    [Fact]
    public void AssignRole_Should_AddRoleToUser()
    {
        //Arrange
        var user = User.Create(UserData.FirstName,UserData.LastName,UserData.Email);
        
        //Act
        user.AssignRole(Role.Registered);
        
        //Assert
        user.Roles.Should().Contain(Role.Registered);
    }

    [Fact]
    public void AssignRole_Should_RaiseUserRoleAssignedDomainEvent()
    {
        //Arrange
        var user = User.Create(UserData.FirstName,UserData.LastName,UserData.Email);
        
        //Act
        user.AssignRole(Role.Registered);
        
        //Assert
        var domainEvent = AssertDomainEventWasPublished<UserRoleAssignedDomainEvent>(user);
        domainEvent.UserId.Should().Be(user.Id);
        domainEvent.RoleId.Should().Be(Role.Registered.Id);
    }



}