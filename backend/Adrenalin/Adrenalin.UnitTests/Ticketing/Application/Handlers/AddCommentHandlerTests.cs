using Adrenalin.Modules.Ticketing.Application.Commands;
using Adrenalin.Modules.Ticketing.Application.Handlers;
using Adrenalin.Modules.Ticketing.Domain.Entities;
using Adrenalin.Modules.Ticketing.Domain.Exceptions;
using Adrenalin.Modules.Ticketing.Domain.Enums;
using Adrenalin.UnitTests.Fakes;

namespace Adrenalin.UnitTests.Ticketing.Application.Handlers;

// Tests for <see cref="AddCommentCommandHandler"/>.
// Covers: agent comment, customer-reply comment, missing ticket.
public class AddCommentHandlerTests
{
    private readonly FakeTicketRepository _repo;
    private readonly AddCommentCommandHandler _handler;

    public AddCommentHandlerTests()
    {
        _repo    = new FakeTicketRepository();
        _handler = new AddCommentCommandHandler(_repo);
    }

    private Ticket CreateAndPin(string subject = "Subject", string desc = "Desc")
    {
        var ticket = Ticket.Create(Guid.NewGuid(), Guid.NewGuid(), subject, desc);
        _repo.TicketToReturn = ticket;
        return ticket;
    }

    // Happy path

    [Fact]
    public async Task Handle_ShouldAddAgentComment_WhenTicketExists()
    {
        // Arrange
        var ticket   = CreateAndPin();
        var authorId = Guid.NewGuid();
        var command  = new AddCommentCommand(ticket.Id, authorId, null, "Agent comment", CommentVisibility.Public);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotEqual(Guid.Empty, result);
        Assert.Single(ticket.TicketComments);

        var comment = ticket.TicketComments.First();
        Assert.Equal("Agent comment", comment.Body);
        Assert.Equal(authorId, comment.AuthorId);
        Assert.Null(comment.ContactId);
        Assert.Equal(CommentVisibility.Public, comment.Visibility);
    }

    [Fact]
    public async Task Handle_ShouldAddCustomerReply_AndNotSetUpdatedBy()
    {
        // Arrange
        var ticket    = CreateAndPin();
        var contactId = Guid.NewGuid();
        var command   = new AddCommentCommand(ticket.Id, null, contactId, "Customer reply", CommentVisibility.Public);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotEqual(Guid.Empty, result);
        Assert.Single(ticket.TicketComments);

        var comment = ticket.TicketComments.First();
        Assert.Equal("Customer reply", comment.Body);
        Assert.Null(comment.AuthorId);
        Assert.Equal(contactId, comment.ContactId);

        // Contact replies must not stamp UpdatedBy (internal field)
        Assert.Null(ticket.UpdatedBy);
    }

    // Guard / error cases

    [Fact]
    public async Task Handle_ShouldThrow_WhenTicketDoesNotExist()
    {
        // Arrange
        _repo.TicketToReturn = null;
        var command = new AddCommentCommand(Guid.NewGuid(), Guid.NewGuid(), null, "Body", CommentVisibility.Public);

        // Act & Assert
        await Assert.ThrowsAsync<TicketDomainException>(() =>
            _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public void TicketComment_ShouldThrow_WhenBothAuthorAndContactProvided()
    {
        Assert.Throws<TicketDomainException>(() =>
            TicketComment.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "Body", CommentVisibility.Public));
    }

    [Fact]
    public void TicketComment_ShouldThrow_WhenCustomerPostsInternalComment()
    {
        Assert.Throws<TicketDomainException>(() =>
            TicketComment.Create(Guid.NewGuid(), null, Guid.NewGuid(), "Body", CommentVisibility.Internal));
    }
}
