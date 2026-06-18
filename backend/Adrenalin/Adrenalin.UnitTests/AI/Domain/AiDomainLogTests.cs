using System;
using Xunit;
using FluentAssertions;
using Adrenalin.Modules.AI.Domain.Entities;

namespace Adrenalin.UnitTests.AI.Domain;

public class AiDomainLogTests
{
    [Fact]
    public void AiSuggestionLog_Create_ShouldInitializeCorrectly()
    {
        var ticketId = Guid.NewGuid();
        var log = AiSuggestionLog.Create(ticketId, "Summary", "prompt123", "response123", "Mock", "v1", 150, 200);

        log.TicketId.Should().Be(ticketId);
        log.RequestType.Should().Be("Summary");
        log.TokensUsed.Should().Be(150);
        log.CreatedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void AutoResolutionLog_Create_ShouldInitializeCorrectly()
    {
        var ticketId = Guid.NewGuid();
        var agentId = Guid.NewGuid();
        var log = AutoResolutionLog.Create(ticketId, "Suggestion text", true, agentId);

        log.TicketId.Should().Be(ticketId);
        log.Suggestion.Should().Be("Suggestion text");
        log.Applied.Should().BeTrue();
        log.AppliedBy.Should().Be(agentId);
        log.CreatedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(1));
    }
}
