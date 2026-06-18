using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Moq;
using FluentAssertions;
using Adrenalin.Modules.AI.Application.Contracts;
using Adrenalin.Modules.AI.Application.Queries;
using Adrenalin.Modules.AI.Application.DTOs;

namespace Adrenalin.UnitTests.AI.Queries;

public class GenerateTicketSummaryQueryHandlerTests
{
    [Fact]
    public async Task Handle_ShouldReturnSummaryDto_WhenProviderReturnsSummary()
    {
        // Arrange
        var mockProvider = new Mock<IAiProvider>();
        mockProvider.Setup(p => p.GenerateSummaryAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("Generated Summary Text");

        var handler = new GenerateTicketSummaryQueryHandler(mockProvider.Object);
        var ticketId = Guid.NewGuid();
        var query = new GenerateTicketSummaryQuery(ticketId);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.TicketId.Should().Be(ticketId);
        result.Summary.Should().Be("Generated Summary Text");
        result.ConfidenceScore.Should().Be(0.95m);
        mockProvider.Verify(p => p.GenerateSummaryAsync(It.Is<string>(s => s.Contains(ticketId.ToString())), It.IsAny<CancellationToken>()), Times.Once);
    }
}
