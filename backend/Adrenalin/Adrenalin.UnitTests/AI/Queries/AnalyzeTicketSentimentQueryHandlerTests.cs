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

public class AnalyzeTicketSentimentQueryHandlerTests
{
    [Fact]
    public async Task Handle_ShouldReturnSentimentDto()
    {
        // Arrange
        var mockProvider = new Mock<IAiProvider>();
        mockProvider.Setup(p => p.AnalyzeSentimentAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SentimentAnalysisResult("Angry", 0.99m, "Customer used caps lock"));

        var handler = new AnalyzeTicketSentimentQueryHandler(mockProvider.Object);
        var ticketId = Guid.NewGuid();
        var query = new AnalyzeTicketSentimentQuery(ticketId);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.TicketId.Should().Be(ticketId);
        result.Sentiment.Should().Be("Angry");
        result.Score.Should().Be(0.99m);
        result.Reasoning.Should().Be("Customer used caps lock");
    }
}
