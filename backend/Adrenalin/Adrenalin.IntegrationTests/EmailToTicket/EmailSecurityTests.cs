using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Xunit;
using FluentAssertions;
using Adrenalin.Persistence.Context;
using Adrenalin.Modules.Ticketing.Domain.Entities;
using Adrenalin.Modules.Ticketing.Domain.Enums;

namespace Adrenalin.IntegrationTests.EmailToTicket;

[Collection("Integration")]
public class EmailSecurityTests : IClassFixture<IntegrationTestWebAppFactory>, IAsyncLifetime
{
    private readonly IntegrationTestWebAppFactory _factory;
    private readonly IServiceScope _scope;
    private readonly AdrenalinDbContext _dbContext;

    public EmailSecurityTests(IntegrationTestWebAppFactory factory)
    {
        _factory = factory;
        _scope = _factory.Services.CreateScope();
        _dbContext = _scope.ServiceProvider.GetRequiredService<AdrenalinDbContext>();
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync()
    {
        _dbContext.Tickets.RemoveRange(_dbContext.Tickets);
        await _dbContext.SaveChangesAsync();
        _scope.Dispose();
    }

    [Fact]
    public async Task CreateTicket_WithSqlInjectionString_ShouldPersistSafely()
    {
        // Arrange
        var sqlInjectionString = "'; DROP TABLE ticket.tickets; --";

        var emailMessage = new Adrenalin.Modules.Ticketing.Domain.Entities.Email.EmailMessage {
            Provider = "IMAP",
            InternetMessageId = $"<{Guid.NewGuid()}@domain.com>",
            MessageId = "sec-msg-123",
            SenderEmail = "sec@test.com",
            SenderName = "Sec User",
            RecipientEmail = "support@adrenalin.com",
            Subject = sqlInjectionString,
            BodyText = "Safe body",
            BodyHtml = "Safe html",
            ReceivedAt = DateTimeOffset.UtcNow,
            ProcessingState = Adrenalin.Modules.Ticketing.Domain.Entities.Email.EmailProcessingState.Processed
        };

        // Act
        _dbContext.Set<Adrenalin.Modules.Ticketing.Domain.Entities.Email.EmailMessage>().Add(emailMessage);
        
        var act = async () => await _dbContext.SaveChangesAsync();

        // Assert
        await act.Should().NotThrowAsync();
        
        var savedMessage = await _dbContext.Set<Adrenalin.Modules.Ticketing.Domain.Entities.Email.EmailMessage>()
            .FirstOrDefaultAsync(x => x.Id == emailMessage.Id);
            
        savedMessage.Should().NotBeNull();
        savedMessage!.Subject.Should().Be(sqlInjectionString);
        savedMessage.BodyText.Should().Be("Safe body");
    }
}
