using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Moq;
using FluentAssertions;
using Adrenalin.Modules.AI.Application.Contracts;
using Adrenalin.Modules.AI.Application.Commands;
using Adrenalin.Modules.AI.Application.DTOs;

namespace Adrenalin.UnitTests.AI.Commands;

public class AutoCategorizeTicketCommandHandlerTests
{
    [Fact]
    public async Task Handle_ShouldReturnCategorizationDto()
    {
        // Arrange
        var mockProvider = new Mock<IAiProvider>();
        mockProvider.Setup(p => p.CategorizeAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AutoCategorizationResult("Billing", "Refund", 0.92m));

        var handler = new AutoCategorizeTicketCommandHandler(mockProvider.Object);
        var ticketId = Guid.NewGuid();
        var command = new AutoCategorizeTicketCommand(ticketId);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.TicketId.Should().Be(ticketId);
        result.Category.Should().Be("Billing");
        result.SubCategory.Should().Be("Refund");
        result.ConfidenceScore.Should().Be(0.92m);
    }
}
