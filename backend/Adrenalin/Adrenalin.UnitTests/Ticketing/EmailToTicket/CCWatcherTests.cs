using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Adrenalin.Modules.Ticketing.Application.Services;
using Adrenalin.SharedKernel.Mediator;
using Adrenalin.SharedKernel.Results;
using FluentAssertions;
using NSubstitute;
using Xunit;
using Adrenalin.Modules.Ticketing.Application.Commands;
using Adrenalin.SharedKernel.Contracts;

namespace Adrenalin.UnitTests.Ticketing.EmailToTicket;

public class CCWatcherTests
{
    private readonly IDispatcher _dispatcher;
    private readonly Microsoft.Extensions.Logging.ILogger<EmailWatcherExtractionService> _logger;
    private readonly EmailWatcherExtractionService _sut;

    public CCWatcherTests()
    {
        _dispatcher = Substitute.For<IDispatcher>();
        _logger = Substitute.For<Microsoft.Extensions.Logging.ILogger<EmailWatcherExtractionService>>();
        _sut = new EmailWatcherExtractionService(_dispatcher, _logger);
    }

    [Fact]
    public async Task ExtractWatchersAsync_ShouldMergeToAndCc_AndExcludeSenderAndSystemEmail()
    {
        // Arrange
        var toEmails = "support@company.com, user1@test.com";
        var ccEmails = new List<string> { "user2@test.com", "sender@test.com" };
        var senderEmail = "sender@test.com";
        var systemEmail = "support@company.com";
        var companyId = Guid.NewGuid();
        var internalCompanyId = Guid.NewGuid();

        var user1Id = Guid.NewGuid();
        var user2Id = Guid.NewGuid();

        // Setup dispatcher to return valid contacts for user1 and user2
        _dispatcher.Send(Arg.Is<ResolveEmailContactContractCommand>(c => c.EmailAddress == "user1@test.com"), Arg.Any<CancellationToken>())
            .Returns(Result<ResolveEmailContactContractResult>.Success(new ResolveEmailContactContractResult(internalCompanyId, user1Id, user1Id, false)));
            
        _dispatcher.Send(Arg.Is<ResolveEmailContactContractCommand>(c => c.EmailAddress == "user2@test.com"), Arg.Any<CancellationToken>())
            .Returns(Result<ResolveEmailContactContractResult>.Success(new ResolveEmailContactContractResult(internalCompanyId, user2Id, user2Id, false)));

        // Act
        var result = await _sut.ExtractWatchersAsync(toEmails, ccEmails, senderEmail, systemEmail, companyId, CancellationToken.None);

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(user1Id);
        result.Should().Contain(user2Id);
    }

    [Fact]
    public async Task ExtractWatchersAsync_ShouldEnforceMaxWatcherLimits()
    {
        // Arrange
        var ccEmailsList = Enumerable.Range(1, 15).Select(i => $"user{i}@test.com").ToList();
        var companyId = Guid.NewGuid();
        var internalCompanyId = Guid.NewGuid();

        foreach (var email in ccEmailsList)
        {
            _dispatcher.Send(Arg.Is<ResolveEmailContactContractCommand>(c => c.EmailAddress == email), Arg.Any<CancellationToken>())
                .Returns(Result<ResolveEmailContactContractResult>.Success(new ResolveEmailContactContractResult(internalCompanyId, Guid.NewGuid(), Guid.NewGuid(), false)));
        }

        // Act
        // Assuming limit is 10
        var result = await _sut.ExtractWatchersAsync(null, ccEmailsList, "sender@test.com", "system@test.com", companyId, CancellationToken.None);

        // Assert
        result.Should().HaveCount(10); // Limit enforced
    }

    [Fact]
    public async Task ExtractWatchersAsync_ShouldIgnoreInvalidEmailFormats()
    {
        // Arrange
        var ccEmails = new List<string> { "valid@test.com", "not_an_email", "@domain.com" };
        var companyId = Guid.NewGuid();
        var validId = Guid.NewGuid();
        var internalCompanyId = Guid.NewGuid();

        _dispatcher.Send(Arg.Is<ResolveEmailContactContractCommand>(c => c.EmailAddress == "valid@test.com"), Arg.Any<CancellationToken>())
            .Returns(Result<ResolveEmailContactContractResult>.Success(new ResolveEmailContactContractResult(internalCompanyId, validId, validId, false)));

        // Act
        var result = await _sut.ExtractWatchersAsync(null, ccEmails, "sender@test.com", "system@test.com", companyId, CancellationToken.None);

        // Assert
        result.Should().HaveCount(1);
        result.Should().Contain(validId);
    }
}
