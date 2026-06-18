using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using FluentAssertions;
using Adrenalin.Persistence.Context;
using Adrenalin.Modules.Ticketing.Domain.Entities.Email;
using Adrenalin.Modules.Ticketing.Domain.Interfaces;
using Adrenalin.Persistence.Repositories;
using System.Linq;

namespace Adrenalin.IntegrationTests.EmailToTicket;

[Collection("Integration")]
public class EmailAliasRoutingIntegrationTests : IClassFixture<IntegrationTestWebAppFactory>, IAsyncLifetime
{
    private readonly IntegrationTestWebAppFactory _factory;
    private readonly IServiceScope _scope;
    private readonly AdrenalinDbContext _dbContext;
    private readonly IEmailAliasRoutingRepository _repository;

    public EmailAliasRoutingIntegrationTests(IntegrationTestWebAppFactory factory)
    {
        _factory = factory;
        _scope = _factory.Services.CreateScope();
        _dbContext = _scope.ServiceProvider.GetRequiredService<AdrenalinDbContext>();
        _repository = new EmailAliasRoutingRepository(_dbContext);
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync()
    {
        _dbContext.EmailAliasRoutes.RemoveRange(_dbContext.EmailAliasRoutes);
        await _dbContext.SaveChangesAsync();
        _scope.Dispose();
    }

    [Fact]
    public async Task GetBestMatchAsync_DirectMatch_ShouldReturnHighestPriority()
    {
        // Arrange
        var companyId = Guid.NewGuid();
        var route1 = new EmailAliasRouting { EmailAddress = "support@test.com", Priority = 10, CompanyId = companyId, GroupId = Guid.NewGuid() };
        var route2 = new EmailAliasRouting { EmailAddress = "support@test.com", Priority = 20, CompanyId = companyId, GroupId = Guid.NewGuid() };
        
        _dbContext.EmailAliasRoutes.AddRange(route1, route2);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _repository.GetBestMatchAsync("support@test.com");

        // Assert
        result.Should().NotBeNull();
        result!.Priority.Should().Be(20);
        result.Id.Should().Be(route2.Id);
    }

    [Fact]
    public async Task GetBestMatchAsync_DomainMatch_ShouldReturnHighestPriorityDomainMatch()
    {
        // Arrange
        var companyId = Guid.NewGuid();
        var route1 = new EmailAliasRouting { EmailAddress = "*@test.com", Priority = 5, CompanyId = companyId, GroupId = Guid.NewGuid() };
        
        _dbContext.EmailAliasRoutes.AddRange(route1);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _repository.GetBestMatchAsync("anything@test.com");

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(route1.Id);
    }

    [Fact]
    public async Task GetBestMatchAsync_FallbackToDefault_ShouldReturnDefaultWhenNoMatch()
    {
        // Arrange
        var companyId = Guid.NewGuid();
        var defaultRoute = new EmailAliasRouting { EmailAddress = "*", Priority = 1, CompanyId = companyId, GroupId = Guid.NewGuid() };
        
        _dbContext.EmailAliasRoutes.AddRange(defaultRoute);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _repository.GetBestMatchAsync("unknown@test.com");

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(defaultRoute.Id);
    }

    [Fact]
    public async Task GetBestMatchAsync_InactiveAlias_ShouldBeIgnored()
    {
        // Arrange
        var companyId = Guid.NewGuid();
        var inactiveRoute = new EmailAliasRouting { EmailAddress = "support@test.com", Priority = 100, CompanyId = companyId, GroupId = Guid.NewGuid() };
        inactiveRoute.Deactivate();
        var activeRoute = new EmailAliasRouting { EmailAddress = "support@test.com", Priority = 10, CompanyId = companyId, GroupId = Guid.NewGuid() };
        
        _dbContext.EmailAliasRoutes.AddRange(inactiveRoute, activeRoute);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _repository.GetBestMatchAsync("support@test.com");

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(activeRoute.Id); // Ignored inactive priority 100
    }
}
