using Adrenalin.Modules.Ticketing.Application.Commands;
using Adrenalin.Modules.Ticketing.Application.Handlers;
using Adrenalin.Modules.Ticketing.Domain.Entities;
using Adrenalin.Modules.Ticketing.Domain.Enums;
using Adrenalin.Modules.Ticketing.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Adrenalin.UnitTests;

public class AddCommentCommandHandlerTests
{
    private class FakeTicketRepository : ITicketRepository
    {
        public Ticket? TicketToReturn { get; set; }

        public Task<Ticket?> GetByIdAsync(Guid ticketId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(TicketToReturn?.Id == ticketId ? TicketToReturn : null);
        }

        public Task AddAsync(Ticket ticket, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public void Update(Ticket ticket) { }
        public Task<bool> ExistsAsync(Guid ticketId, CancellationToken cancellationToken = default) => Task.FromResult(TicketToReturn?.Id == ticketId);
        public void Remove(Ticket ticket) { }

        public Task<IReadOnlyList<Ticket>> GetTicketsAsync(string? ticketNumber, TicketStatus? status, Guid? assignedAgentId, Guid? companyId, int page, int pageSize, CancellationToken cancellationToken)
        {
            return Task.FromResult<IReadOnlyList<Ticket>>(Array.Empty<Ticket>());
        }

        public Task<int> CountTicketsAsync(string? ticketNumber, TicketStatus? status, Guid? assignedAgentId, Guid? companyId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(0);
        }

        public Guid? DefaultCompanyId { get; set; }
        
        public Dictionary<Guid, Guid> UserCompanyMap { get; } = new();

        public Task<Guid?> GetUserCompanyIdAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            if (UserCompanyMap.TryGetValue(userId, out var cid))
            {
                return Task.FromResult<Guid?>(cid);
            }
            if (DefaultCompanyId.HasValue)
            {
                return Task.FromResult<Guid?>(DefaultCompanyId.Value);
            }
            if (TicketToReturn is not null)
            {
                return Task.FromResult<Guid?>(TicketToReturn.CompanyId);
            }
            return Task.FromResult<Guid?>(null);
        }
    }

    private readonly FakeTicketRepository _ticketRepository;
    private readonly AddCommentCommandHandler _handler;

    public AddCommentCommandHandlerTests()
    {
        _ticketRepository = new FakeTicketRepository();
        _handler = new AddCommentCommandHandler(_ticketRepository);
    }

    [Fact]
    public async Task Handle_ShouldAddComment_WhenTicketExists()
    {
        // Arrange
        var companyId = Guid.NewGuid();
        var moduleId = Guid.NewGuid();
        var ticket = Ticket.Create(companyId, moduleId, "Test Subject", "Test Description");
        
        _ticketRepository.TicketToReturn = ticket;

        var authorId = Guid.NewGuid();
        var command = new AddCommentCommand(
            ticket.Id,
            authorId,
            null,
            "This is a comment",
            CommentVisibility.Public
        );

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotEqual(Guid.Empty, result);
        Assert.Single(ticket.TicketComments);
        
        var comment = ticket.TicketComments.GetEnumerator();
        comment.MoveNext();
        Assert.Equal("This is a comment", comment.Current.Body);
        Assert.Equal(authorId, comment.Current.AuthorId);
        Assert.Null(comment.Current.ContactId);
        Assert.Equal(CommentVisibility.Public, comment.Current.Visibility);
    }

    [Fact]
    public async Task Handle_ShouldAddCommentAndTouchNull_WhenCommentIsCustomerReply()
    {
        // Arrange
        var companyId = Guid.NewGuid();
        var moduleId = Guid.NewGuid();
        var ticket = Ticket.Create(companyId, moduleId, "Test Subject", "Test Description");
        
        _ticketRepository.TicketToReturn = ticket;

        var contactId = Guid.NewGuid();
        var command = new AddCommentCommand(
            ticket.Id,
            null,
            contactId,
            "This is a customer reply comment",
            CommentVisibility.Public
        );

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotEqual(Guid.Empty, result);
        Assert.Single(ticket.TicketComments);
        
        var comment = ticket.TicketComments.GetEnumerator();
        comment.MoveNext();
        Assert.Equal("This is a customer reply comment", comment.Current.Body);
        Assert.Null(comment.Current.AuthorId);
        Assert.Equal(contactId, comment.Current.ContactId);
        Assert.Equal(CommentVisibility.Public, comment.Current.Visibility);
        
        // Ticket UpdatedBy should remain null (since it was updated by a customer Contact, not a internal User)
        Assert.Null(ticket.UpdatedBy);
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenTicketDoesNotExist()
    {
        // Arrange
        var ticketId = Guid.NewGuid();
        _ticketRepository.TicketToReturn = null;

        var command = new AddCommentCommand(
            ticketId,
            Guid.NewGuid(),
            null,
            "Comment body",
            CommentVisibility.Public
        );

        // Act & Assert
        await Assert.ThrowsAsync<TicketDomainException>(() => 
            _handler.Handle(command, CancellationToken.None));
    }
}
