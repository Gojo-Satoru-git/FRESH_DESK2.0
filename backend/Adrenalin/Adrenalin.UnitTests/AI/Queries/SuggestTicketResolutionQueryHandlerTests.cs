using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Moq;
using FluentAssertions;
using Adrenalin.Modules.AI.Application.Contracts;
using Adrenalin.Modules.AI.Application.Queries;
using Adrenalin.Modules.AI.Application.DTOs;

namespace Adrenalin.UnitTests.AI.Queries;

public class SuggestTicketResolutionQueryHandlerTests
{
    [Fact]
    public async Task Handle_ShouldReturnResolutionSuggestionDto()
    {
        // Arrange
        var mockProvider = new Mock<IAiProvider>();
        mockProvider.Setup(p => p.GenerateResolutionSuggestionAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("Restart router");
        mockProvider.Setup(p => p.SuggestKnowledgeBaseArticlesAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<string> { "KB-123" });

        var handler = new SuggestTicketResolutionQueryHandler(mockProvider.Object);
        var ticketId = Guid.NewGuid();
        var query = new SuggestTicketResolutionQuery(ticketId);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.TicketId.Should().Be(ticketId);
        result.SuggestedReply.Should().Be("Restart router");
        result.SuggestedKbArticles.Should().ContainSingle().Which.Should().Be("KB-123");
    }
}
