using Adrenalin.Modules.Ticketing.Domain.Entities.Email;
using Adrenalin.Persistence.Context;
using Adrenalin.SharedKernel.Mediator;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Moq;

namespace Adrenalin.UnitTests.Ticketing.EmailToTicket;

public class EmailSchemaValidationTests
{
    private readonly IModel _model;

    public EmailSchemaValidationTests()
    {
        var options = new DbContextOptionsBuilder<AdrenalinDbContext>()
            .UseNpgsql("Host=ep-patient-wave-ap4xzb3s-pooler.c-7.us-east-1.aws.neon.tech; Database=Adrenalin-DB; Username=neondb_owner; Password=npg_0ShejQEoA4wP")
            .UseSnakeCaseNamingConvention()
            .Options;

        var mockPublisher = new Mock<IPublisher>();
        var context = new AdrenalinDbContext(options, mockPublisher.Object);
        _model = context.Model;
    }

    [Fact]
    public void EmailMessage_ShouldMapTo_TicketSchema_And_EmailMessagesTable()
    {
        var entityType = _model.FindEntityType(typeof(EmailMessage));
        entityType.Should().NotBeNull();

        entityType!.GetTableName().Should().Be("email_messages");
        entityType.GetSchema().Should().Be("ticket");
    }

    [Fact]
    public void EmailAttachment_ShouldMapTo_TicketSchema_And_EmailAttachmentsTable()
    {
        var entityType = _model.FindEntityType(typeof(EmailAttachment));
        entityType.Should().NotBeNull();

        entityType!.GetTableName().Should().Be("email_attachments");
        entityType.GetSchema().Should().Be("ticket");
    }

    [Fact]
    public void ProcessedEmailLog_ShouldMapTo_TicketSchema_And_ProcessedEmailLogsTable()
    {
        var entityType = _model.FindEntityType(typeof(ProcessedEmailLog));
        entityType.Should().NotBeNull();

        entityType!.GetTableName().Should().Be("processed_email_logs");
        entityType.GetSchema().Should().Be("ticket");
    }

    [Fact]
    public void EmailAliasRouting_ShouldMapTo_ConfigSchema_And_EmailAliasRoutesTable()
    {
        var entityType = _model.FindEntityType(typeof(EmailAliasRouting));
        entityType.Should().NotBeNull();

        entityType!.GetTableName().Should().Be("email_alias_routes");
        entityType.GetSchema().Should().Be("config");
    }
}
