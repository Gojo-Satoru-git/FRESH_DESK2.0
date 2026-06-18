using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Xunit;
using FluentAssertions;
using Adrenalin.Persistence.Context;
using Adrenalin.Modules.Ticketing.Domain.Entities.Email;
using Adrenalin.Modules.Ticketing.Domain.Interfaces;
using Adrenalin.Persistence.Repositories;
using Npgsql;

namespace Adrenalin.IntegrationTests.EmailToTicket;

[Collection("Integration")]
public class EmailConcurrencyTests : IClassFixture<IntegrationTestWebAppFactory>, IAsyncLifetime
{
    private readonly IntegrationTestWebAppFactory _factory;
    private readonly IServiceScope _scope;
    private readonly AdrenalinDbContext _dbContext;
    private readonly IProcessedEmailLogRepository _repository;

    public EmailConcurrencyTests(IntegrationTestWebAppFactory factory)
    {
        _factory = factory;
        _scope = _factory.Services.CreateScope();
        _dbContext = _scope.ServiceProvider.GetRequiredService<AdrenalinDbContext>();
        _repository = new ProcessedEmailLogRepository(_dbContext);
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync()
    {
        _dbContext.ProcessedEmailLogs.RemoveRange(_dbContext.ProcessedEmailLogs);
        await _dbContext.SaveChangesAsync();
        _scope.Dispose();
    }

    [Fact]
    public async Task ProcessedEmailLog_ShouldEnforce_UniqueConstraint_UnderConcurrency()
    {
        // Arrange
        var internetMessageId = $"<concurrent-test-{Guid.NewGuid()}@domain.com>";
        var threads = 50;
        
        var tasks = new List<Task>();
        var exceptions = new System.Collections.Concurrent.ConcurrentBag<Exception>();
        var successes = 0;

        // Act
        for (int i = 0; i < threads; i++)
        {
            tasks.Add(Task.Run(async () =>
            {
                using var scope = _factory.Services.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<AdrenalinDbContext>();
                
                try
                {
                    var log = new ProcessedEmailLog { InternetMessageId = internetMessageId, Provider = "IntegrationTest" };
                    dbContext.ProcessedEmailLogs.Add(log);
                    await dbContext.SaveChangesAsync();
                    Interlocked.Increment(ref successes);
                }
                catch (Exception ex)
                {
                    exceptions.Add(ex);
                }
            }));
        }

        await Task.WhenAll(tasks);

        // Assert
        // We expect exactly ONE success because PostgreSQL enforces a unique constraint on InternetMessageId
        successes.Should().Be(1);

        // All other attempts should have failed with a PostgresException for unique constraint violation
        exceptions.Should().HaveCount(threads - 1);
        exceptions.All(e => e.InnerException is PostgresException pe && pe.SqlState == "23505").Should().BeTrue();
    }
}
