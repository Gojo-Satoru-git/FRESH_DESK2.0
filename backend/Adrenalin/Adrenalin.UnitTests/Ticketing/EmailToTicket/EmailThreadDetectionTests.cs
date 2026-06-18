using Adrenalin.EventBus.Events;
using Adrenalin.Modules.Ticketing.Application.Services;
using Adrenalin.Modules.Ticketing.Domain.Entities;
using Adrenalin.Modules.Ticketing.Domain.Entities.Email;
using Adrenalin.Modules.Ticketing.Domain.Enums;
using Adrenalin.Modules.Ticketing.Domain.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Adrenalin.UnitTests.Ticketing.EmailToTicket;

public class EmailThreadDetectionTests
{
    private readonly Mock<IEmailMessageRepository> _emailMessageRepositoryMock;
    private readonly Mock<ITicketRepository> _ticketRepositoryMock;
    private readonly Mock<ILogger<EmailThreadDetectionService>> _loggerMock;
    private readonly EmailThreadDetectionService _service;
    private readonly Guid _tenantCompanyId = Guid.NewGuid();

    public EmailThreadDetectionTests()
    {
        _emailMessageRepositoryMock = new Mock<IEmailMessageRepository>();
        _ticketRepositoryMock = new Mock<ITicketRepository>();
        _loggerMock = new Mock<ILogger<EmailThreadDetectionService>>();
        
        _service = new EmailThreadDetectionService(
            _emailMessageRepositoryMock.Object,
            _ticketRepositoryMock.Object,
            _loggerMock.Object
        );
    }

    [Fact]
    public async Task DetectThreadAsync_WithNullReferencesAndInReplyTo_ShouldReturnNull()
    {
        // Arrange — no subject ticket number, no InReplyTo, no References, 
        // InternetMessageId not found, ThreadId not found
        var integrationEvent = CreateEvent(inReplyTo: null, references: null, subject: "Plain subject");

        _emailMessageRepositoryMock
            .Setup(x => x.GetByInternetMessageIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((EmailMessage?)null);

        _emailMessageRepositoryMock
            .Setup(x => x.GetByThreadIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<EmailMessage>());

        // Act
        var result = await _service.DetectThreadAsync(integrationEvent, _tenantCompanyId, null, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task DetectThreadAsync_WithInReplyToMatchingExistingTicket_ShouldReturnTicketId()
    {
        // Arrange
        var expectedTicketId = Guid.NewGuid();
        var inReplyTo = "<parent@domain.com>";
        var integrationEvent = CreateEvent(inReplyTo: inReplyTo, references: null);

        var existingMessage = new EmailMessage { TicketId = expectedTicketId };
        _emailMessageRepositoryMock.Setup(x => x.GetByInternetMessageIdAsync(inReplyTo, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingMessage);

        var ticket = Ticket.Create(_tenantCompanyId, Guid.NewGuid(), "Test Subject", "Test Description", TicketType.Incident);
        // Override Id via reflection since it's set internally
        typeof(Ticket).GetProperty("Id")!.DeclaringType!.GetProperty("Id")!.SetValue(ticket, expectedTicketId);
        _ticketRepositoryMock.Setup(x => x.GetByIdAsync(expectedTicketId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ticket);

        // Act
        var result = await _service.DetectThreadAsync(integrationEvent, _tenantCompanyId, null, CancellationToken.None);

        // Assert
        result.Should().Be(expectedTicketId);
    }

    [Fact]
    public async Task DetectThreadAsync_WithInReplyToMatchingDifferentTenant_ShouldReturnNull()
    {
        // Arrange — InReplyTo matches a ticket owned by a DIFFERENT company
        var otherCompanyTicketId = Guid.NewGuid();
        var otherCompanyId = Guid.NewGuid();
        var inReplyTo = "<parent@domain.com>";
        var integrationEvent = CreateEvent(inReplyTo: inReplyTo, references: null);

        var existingMessage = new EmailMessage { TicketId = otherCompanyTicketId };
        _emailMessageRepositoryMock.Setup(x => x.GetByInternetMessageIdAsync(inReplyTo, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingMessage);

        var otherTicket = Ticket.Create(otherCompanyId, Guid.NewGuid(), "Other Ticket", "Other Description", TicketType.Incident);
        typeof(Ticket).GetProperty("Id")!.DeclaringType!.GetProperty("Id")!.SetValue(otherTicket, otherCompanyTicketId);
        _ticketRepositoryMock.Setup(x => x.GetByIdAsync(otherCompanyTicketId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(otherTicket);

        // InternetMessageId & ThreadId also return nothing
        _emailMessageRepositoryMock.Setup(x => x.GetByInternetMessageIdAsync(It.Is<string>(s => s != inReplyTo), It.IsAny<CancellationToken>()))
            .ReturnsAsync((EmailMessage?)null);
        _emailMessageRepositoryMock.Setup(x => x.GetByThreadIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<EmailMessage>());

        // Act
        var result = await _service.DetectThreadAsync(integrationEvent, _tenantCompanyId, null, CancellationToken.None);

        // Assert — must NOT return the other company's ticket
        result.Should().BeNull();
    }

    [Theory]
    [InlineData("RE: TKT-2026-123456 Issue", "TKT-2026-123456")]
    [InlineData("FW: FW: TKT-2026-654321 Broken", "TKT-2026-654321")]
    [InlineData("Customer changed subject completely TKT-2026-111111 !!", "TKT-2026-111111")]
    public async Task DetectThreadAsync_WithTicketNumberInSubject_ShouldReturnTicketId(string subject, string expectedTicketNumber)
    {
        var ticket = Ticket.Create(_tenantCompanyId, Guid.NewGuid(), "mock subject", "mock desc", TicketType.Incident);
        var integrationEvent = CreateEvent(inReplyTo: "<msg-456@domain.com>", references: null, subject: subject);

        _ticketRepositoryMock.Setup(x => x.GetByTicketNumberAsync(expectedTicketNumber, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ticket);

        // Act
        var result = await _service.DetectThreadAsync(integrationEvent, _tenantCompanyId, null, CancellationToken.None);

        // Assert — Subject ticket number has priority over InReplyTo
        result.Should().Be(ticket.Id);
    }

    [Fact]
    public async Task DetectThreadAsync_WithSpoofedTicketNumber_CrossTenant_ShouldNotReturnTicketId()
    {
        // Arrange — Subject contains ticket number for a DIFFERENT company's ticket
        var spoofedTicketNumber = "TKT-2026-999999";
        var otherCompanyId = Guid.NewGuid();
        var integrationEvent = CreateEvent(inReplyTo: null, references: null, subject: $"RE: {spoofedTicketNumber}");

        var otherCompanyTicket = Ticket.Create(otherCompanyId, Guid.NewGuid(), "Other ticket", "Other desc", TicketType.Incident);
        _ticketRepositoryMock.Setup(x => x.GetByTicketNumberAsync(spoofedTicketNumber, It.IsAny<CancellationToken>()))
            .ReturnsAsync(otherCompanyTicket);

        _emailMessageRepositoryMock.Setup(x => x.GetByInternetMessageIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((EmailMessage?)null);
        _emailMessageRepositoryMock.Setup(x => x.GetByThreadIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<EmailMessage>());

        // Act
        var result = await _service.DetectThreadAsync(integrationEvent, _tenantCompanyId, null, CancellationToken.None);

        // Assert — must NOT return another company's ticket
        result.Should().BeNull();
    }

    [Fact]
    public async Task DetectThreadAsync_WithSpoofedTicketNumber_SameCompany_FallbackToInReplyTo()
    {
        // Arrange — Subject ticket doesn't exist, but InReplyTo matches a same-tenant ticket
        var spoofedTicketNumber = "TKT-2026-999999";
        var expectedTicketId = Guid.NewGuid();
        var integrationEvent = CreateEvent(inReplyTo: "<valid-parent@domain.com>", references: null, subject: $"RE: {spoofedTicketNumber}");

        _ticketRepositoryMock.Setup(x => x.GetByTicketNumberAsync(spoofedTicketNumber, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Ticket?)null);

        var existingMessage = new EmailMessage { TicketId = expectedTicketId };
        _emailMessageRepositoryMock.Setup(x => x.GetByInternetMessageIdAsync("<valid-parent@domain.com>", It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingMessage);

        var ticket = Ticket.Create(_tenantCompanyId, Guid.NewGuid(), "Valid ticket", "Valid desc", TicketType.Incident);
        typeof(Ticket).GetProperty("Id")!.DeclaringType!.GetProperty("Id")!.SetValue(ticket, expectedTicketId);
        _ticketRepositoryMock.Setup(x => x.GetByIdAsync(expectedTicketId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ticket);

        var result = await _service.DetectThreadAsync(integrationEvent, _tenantCompanyId, null, CancellationToken.None);

        result.Should().Be(expectedTicketId);
    }

    [Fact]
    public async Task DetectThreadAsync_WithMultipleReferences_ShouldSelectNewestSameTenant()
    {
        // Arrange
        var expectedTicketId = Guid.NewGuid();
        var otherCompanyTicketId = Guid.NewGuid();
        var otherCompanyId = Guid.NewGuid();
        var refs = "<msg1@domain.com> <msg2@domain.com>";
        var integrationEvent = CreateEvent(inReplyTo: null, references: refs);

        // msg1 belongs to other company
        _emailMessageRepositoryMock.Setup(x => x.GetByInternetMessageIdAsync("<msg1@domain.com>", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new EmailMessage { TicketId = otherCompanyTicketId, ReceivedAt = DateTimeOffset.UtcNow.AddDays(-1) });
        var otherTicket = Ticket.Create(otherCompanyId, Guid.NewGuid(), "Other ticket", "Other description", TicketType.Incident);
        typeof(Ticket).GetProperty("Id")!.DeclaringType!.GetProperty("Id")!.SetValue(otherTicket, otherCompanyTicketId);
        _ticketRepositoryMock.Setup(x => x.GetByIdAsync(otherCompanyTicketId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(otherTicket);

        // msg2 belongs to our company
        _emailMessageRepositoryMock.Setup(x => x.GetByInternetMessageIdAsync("<msg2@domain.com>", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new EmailMessage { TicketId = expectedTicketId, ReceivedAt = DateTimeOffset.UtcNow });
        var ownTicket = Ticket.Create(_tenantCompanyId, Guid.NewGuid(), "Own ticket", "Own description", TicketType.Incident);
        typeof(Ticket).GetProperty("Id")!.DeclaringType!.GetProperty("Id")!.SetValue(ownTicket, expectedTicketId);
        _ticketRepositoryMock.Setup(x => x.GetByIdAsync(expectedTicketId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ownTicket);

        // InternetMessageId fallback
        _emailMessageRepositoryMock.Setup(x => x.GetByInternetMessageIdAsync(It.Is<string>(s => s != "<msg1@domain.com>" && s != "<msg2@domain.com>"), It.IsAny<CancellationToken>()))
            .ReturnsAsync((EmailMessage?)null);
        _emailMessageRepositoryMock.Setup(x => x.GetByThreadIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<EmailMessage>());

        // Act
        var result = await _service.DetectThreadAsync(integrationEvent, _tenantCompanyId, null, CancellationToken.None);

        // Assert — must return our company's ticket, not the other company's
        result.Should().Be(expectedTicketId);
    }

    private EmailReceivedIntegrationEvent CreateEvent(string? inReplyTo, string? references, string subject = "Re: Help")
    {
        return new EmailReceivedIntegrationEvent(
            Guid.NewGuid(),
            "IMAP",
            "msg-id",
            "<msg-123@domain.com>",
            "thread-id",
            inReplyTo,
            references,
            subject,
            "Body",
            null,
            "sender@domain.com",
            "Sender",
            "ToEmail",
            Array.Empty<string>(),
            DateTimeOffset.UtcNow,
            new System.Collections.Generic.Dictionary<string, string>(),
            Array.Empty<EmailAttachmentDto>()
        );
    }
}
