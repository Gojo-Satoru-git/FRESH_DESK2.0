using Adrenalin.Modules.Ticketing.Domain.Entities.Email;
using Adrenalin.Modules.Ticketing.Domain.Entities;
using Adrenalin.Modules.Ticketing.Domain.Enums;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Adrenalin.IntegrationTests.EmailToTicket;

public class EmailPersistenceIntegrationTests : BaseIntegrationTest
{
    public EmailPersistenceIntegrationTests(IntegrationTestWebAppFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task InsertEmailMessage_WithAttachment_ShouldPersistSuccessfully()
    {
        var emailMessage = new EmailMessage {
            Provider = "IMAP",
            InternetMessageId = "<msg-123@domain.com>",
            MessageId = "msg-123",
            SenderEmail = "test@domain.com",
            SenderName = "Test User",
            RecipientEmail = "support@adrenalin.com",
            Subject = "Need help",
            BodyText = "My body",
            BodyHtml = "<html/>",
            ReceivedAt = DateTimeOffset.UtcNow,
            ProcessingState = EmailProcessingState.Processed
        };

        // Act
        await DbContext.Set<EmailMessage>().AddAsync(emailMessage);
        await DbContext.SaveChangesAsync();

        var attachment = new EmailAttachment {
            EmailMessageId = emailMessage.Id,
            FileName = "error.png",
            ContentType = "image/png",
            Size = 1024,
            StoragePath = "url_to_blob"
        };

        await DbContext.Set<EmailAttachment>().AddAsync(attachment);
        await DbContext.SaveChangesAsync();

        // Assert
        var dbMessage = await DbContext.Set<EmailMessage>()
            .Include(m => m.Attachments)
            .FirstOrDefaultAsync(m => m.Id == emailMessage.Id);

        dbMessage.Should().NotBeNull();
        dbMessage!.TicketId.Should().BeNull();
        dbMessage.Attachments.Should().HaveCount(1);
        dbMessage.Attachments.First().FileName.Should().Be("error.png");
    }

    [Fact]
    public async Task InsertProcessedEmailLog_WithDuplicateInternetMessageId_ShouldThrowUniqueConstraintException()
    {
        // Arrange
        var internetMessageId = "<dup-123@domain.com>";
        var log1 = new ProcessedEmailLog { 
            InternetMessageId = internetMessageId, 
            Provider = "IMAP",
            Status = EmailProcessingState.Processed 
        };
        var log2 = new ProcessedEmailLog { 
            InternetMessageId = internetMessageId, 
            Provider = "IMAP",
            Status = EmailProcessingState.Failed,
            FailureReason = "Duplicate"
        };

        await DbContext.Set<ProcessedEmailLog>().AddAsync(log1);
        await DbContext.SaveChangesAsync();

        // Act
        await DbContext.Set<ProcessedEmailLog>().AddAsync(log2);
        
        // Assert
        var act = async () => await DbContext.SaveChangesAsync();
        await act.Should().ThrowAsync<DbUpdateException>()
            .Where(e => e.InnerException != null && e.InnerException.Message.Contains("duplicate key value violates unique constraint"));
    }

    [Fact]
    public async Task InsertEmailAliasRouting_ShouldPersistSuccessfully()
    {
        // Arrange
        var emailAlias = new EmailAliasRouting {
            EmailAddress = "vip-support@adrenalin.com",
            GroupId = Guid.NewGuid(),
            CompanyId = Guid.NewGuid(),
            Priority = 1
        };

        // Act
        await DbContext.Set<EmailAliasRouting>().AddAsync(emailAlias);
        await DbContext.SaveChangesAsync();

        // Assert
        var dbAlias = await DbContext.Set<EmailAliasRouting>().FindAsync(emailAlias.Id);
        dbAlias.Should().NotBeNull();
        dbAlias!.EmailAddress.Should().Be("vip-support@adrenalin.com");
    }
}
