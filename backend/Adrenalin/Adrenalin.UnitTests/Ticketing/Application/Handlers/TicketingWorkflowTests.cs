using Adrenalin.Modules.Ticketing.Application.Commands;
using Adrenalin.Modules.Ticketing.Application.Handlers;
using Adrenalin.Modules.Ticketing.Application.Queries;
using Adrenalin.Modules.Ticketing.Application.DTOs;
using Adrenalin.Modules.Ticketing.Domain.Entities;
using Adrenalin.Modules.Ticketing.Domain.Enums;
using Adrenalin.Modules.Ticketing.Domain.Interfaces;
using Adrenalin.SharedKernel.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Adrenalin.UnitTests.Ticketing.Application.Handlers;

public class TicketingWorkflowTests
{
    private class FakeTicketRepository : ITicketRepository
    {
        public List<Ticket> Tickets { get; } = new();

        public Task<Ticket?> GetByIdAsync(Guid ticketId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Tickets.FirstOrDefault(x => x.Id == ticketId));
        }

        public Task AddAsync(Ticket ticket, CancellationToken cancellationToken = default)
        {
            Tickets.Add(ticket);
            return Task.CompletedTask;
        }

        public void Update(Ticket ticket) { }

        public Task<bool> ExistsAsync(Guid ticketId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Tickets.Any(x => x.Id == ticketId));
        }

        public void Remove(Ticket ticket)
        {
            Tickets.Remove(ticket);
        }

        public Task<IReadOnlyList<Ticket>> GetTicketsAsync(string? ticketNumber, TicketStatus? status, Guid? assignedAgentId, Guid? companyId, int page, int pageSize, CancellationToken cancellationToken)
        {
            var query = Tickets.AsQueryable();
            if (companyId.HasValue)
            {
                query = query.Where(x => x.CompanyId == companyId);
            }
            return Task.FromResult<IReadOnlyList<Ticket>>(query.ToList());
        }

        public Task<int> CountTicketsAsync(string? ticketNumber, TicketStatus? status, Guid? assignedAgentId, Guid? companyId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Tickets.Count);
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
            if (Tickets.Any())
            {
                return Task.FromResult<Guid?>(Tickets.First().CompanyId);
            }
            return Task.FromResult<Guid?>(null);
        }
    }

    private readonly FakeTicketRepository _ticketRepository;

    public TicketingWorkflowTests()
    {
        _ticketRepository = new FakeTicketRepository();
    }

    [Fact]
    public async Task CloseTicket_ShouldSucceed_WhenTicketIsResolved()
    {
        // Arrange
        var companyId = Guid.NewGuid();
        var moduleId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var ticket = Ticket.Create(companyId, moduleId, "Subj", "Desc");
        
        // Walk through valid transitions to Resolve
        ticket.ChangeStatus(TicketStatus.Open, userId);
        ticket.MarkCustomerCallTaken(userId);
        ticket.ProvideRootCauseAnalysis("Fixed", userId);
        ticket.Resolve(userId, "Resolved");
        await _ticketRepository.AddAsync(ticket);

        var command = new CloseTicketCommand(ticket.Id, userId, "Done");
        var handler = new CloseTicketCommandHandler(_ticketRepository);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(ticket.Id, result);
        Assert.Equal(TicketStatus.Closed, ticket.Status);
    }

    [Fact]
    public async Task CloseTicket_ShouldThrow_WhenTicketIsNotResolved()
    {
        // Arrange
        var companyId = Guid.NewGuid();
        var moduleId = Guid.NewGuid();
        var ticket = Ticket.Create(companyId, moduleId, "Subj", "Desc");
        await _ticketRepository.AddAsync(ticket);

        var command = new CloseTicketCommand(ticket.Id, Guid.NewGuid(), "Done");
        var handler = new CloseTicketCommandHandler(_ticketRepository);

        // Act & Assert
        await Assert.ThrowsAsync<TicketDomainException>(() => handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task ReopenTicket_ShouldSucceed_WhenTicketIsResolvedOrClosed()
    {
        // Arrange
        var companyId = Guid.NewGuid();
        var moduleId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var ticket = Ticket.Create(companyId, moduleId, "Subj", "Desc");
        ticket.ChangeStatus(TicketStatus.Open, userId);
        ticket.MarkCustomerCallTaken(userId);
        ticket.ProvideRootCauseAnalysis("Fixed", userId);
        ticket.Resolve(userId, "Resolved");
        await _ticketRepository.AddAsync(ticket);

        var command = new ReopenTicketCommand(ticket.Id, userId, "Need more help");
        var handler = new ReopenTicketCommandHandler(_ticketRepository);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(ticket.Id, result);
        Assert.Equal(TicketStatus.Reopened, ticket.Status);
    }

    [Fact]
    public async Task ResolveTicket_ShouldSucceed_WhenCallTakenAndRcaProvided()
    {
        // Arrange
        var companyId = Guid.NewGuid();
        var moduleId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var ticket = Ticket.Create(companyId, moduleId, "Subj", "Desc");
        ticket.ChangeStatus(TicketStatus.Open, userId);
        ticket.MarkCustomerCallTaken(userId);
        ticket.ProvideRootCauseAnalysis("Fix detailed", userId);
        await _ticketRepository.AddAsync(ticket);

        var command = new ResolveTicketCommand(ticket.Id, userId, "Resolution notes");
        var handler = new ResolveTicketCommandHandler(_ticketRepository);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(ticket.Id, result);
        Assert.Equal(TicketStatus.Resolved, ticket.Status);
    }

    [Fact]
    public async Task AddRelation_ShouldSucceed_WhenTicketsBelongToSameCompany()
    {
        // Arrange
        var companyId = Guid.NewGuid();
        var moduleId = Guid.NewGuid();
        var ticket1 = Ticket.Create(companyId, moduleId, "Subj1", "Desc1");
        var ticket2 = Ticket.Create(companyId, moduleId, "Subj2", "Desc2");
        await _ticketRepository.AddAsync(ticket1);
        await _ticketRepository.AddAsync(ticket2);

        var command = new AddRelationCommand(ticket1.Id, ticket2.Id, TicketRelationType.DependsOn);
        var handler = new AddRelationCommandHandler(_ticketRepository);

        // Act
        var relationId = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotEqual(Guid.Empty, relationId);
        Assert.Contains(ticket1.TicketRelations, r => r.ChildTicketId == ticket2.Id && r.RelationType == TicketRelationType.DependsOn);
    }

    [Fact]
    public async Task AddRelation_ShouldThrow_WhenTicketsBelongToDifferentCompanies()
    {
        // Arrange
        var company1 = Guid.NewGuid();
        var company2 = Guid.NewGuid();
        var moduleId = Guid.NewGuid();
        var ticket1 = Ticket.Create(company1, moduleId, "Subj1", "Desc1");
        var ticket2 = Ticket.Create(company2, moduleId, "Subj2", "Desc2");
        await _ticketRepository.AddAsync(ticket1);
        await _ticketRepository.AddAsync(ticket2);

        var command = new AddRelationCommand(ticket1.Id, ticket2.Id, TicketRelationType.DependsOn);
        var handler = new AddRelationCommandHandler(_ticketRepository);

        // Act & Assert
        await Assert.ThrowsAsync<TicketDomainException>(() => handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task LinkTickets_ShouldThrow_WhenTicketsBelongToDifferentCompanies()
    {
        // Arrange
        var company1 = Guid.NewGuid();
        var company2 = Guid.NewGuid();
        var moduleId = Guid.NewGuid();
        var ticket1 = Ticket.Create(company1, moduleId, "Subj1", "Desc1");
        var ticket2 = Ticket.Create(company2, moduleId, "Subj2", "Desc2");
        await _ticketRepository.AddAsync(ticket1);
        await _ticketRepository.AddAsync(ticket2);

        var command = new LinkTicketsCommand(ticket1.Id, ticket2.Id, TicketRelationType.DependsOn);
        var handler = new LinkTicketsCommandHandler(_ticketRepository);

        // Act & Assert
        await Assert.ThrowsAsync<TicketDomainException>(() => handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task MergeTicket_ShouldThrow_WhenTicketsBelongToDifferentCompanies()
    {
        // Arrange
        var company1 = Guid.NewGuid();
        var company2 = Guid.NewGuid();
        var moduleId = Guid.NewGuid();
        var master = Ticket.Create(company1, moduleId, "Master", "Desc");
        var duplicate = Ticket.Create(company2, moduleId, "Duplicate", "Desc");
        await _ticketRepository.AddAsync(master);
        await _ticketRepository.AddAsync(duplicate);

        var fakeUow = new FakeUnitOfWork();
        var command = new MergeTicketCommand(master.Id, duplicate.Id, Guid.NewGuid());
        var handler = new MergeTicketCommandHandler(_ticketRepository, fakeUow);

        // Act & Assert
        await Assert.ThrowsAsync<TicketDomainException>(() => handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task GetTicketHistory_ShouldReturnCorrectData()
    {
        // Arrange
        var companyId = Guid.NewGuid();
        var moduleId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var ticket = Ticket.Create(companyId, moduleId, "Subj", "Desc");
        ticket.ChangeStatus(TicketStatus.Open, userId, "Open reason");
        ticket.AssignAgent(Guid.NewGuid(), userId, "Assign notes");
        await _ticketRepository.AddAsync(ticket);

        var query = new GetTicketHistoryQuery(ticket.Id);
        var handler = new GetTicketHistoryQueryHandler(_ticketRepository);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result.StatusHistory);
        Assert.NotEmpty(result.AssignmentLogs);
        Assert.Equal(TicketStatus.Open.ToString(), result.StatusHistory.First().ToStatus);
    }

    [Fact]
    public async Task SplitTicket_ShouldSucceed_AndTransferEntities()
    {
        // Arrange
        var companyId = Guid.NewGuid();
        var moduleId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var originalTicket = Ticket.Create(companyId, moduleId, "Original Subj", "Original Desc");
        
        var comment = TicketComment.Create(originalTicket.Id, userId, null, "Move this comment", CommentVisibility.Internal);
        originalTicket.AddComment(comment, userId);

        var attachment = TicketAttachment.Create(originalTicket.Id, null, "file.txt", "url", 1024, "text/plain", userId);
        originalTicket.AddAttachment(attachment);

        await _ticketRepository.AddAsync(originalTicket);

        var command = new SplitTicketCommand(
            originalTicket.Id,
            "New Subj",
            "New Desc",
            userId,
            CommentIdsToMove: new List<Guid> { comment.Id },
            AttachmentIdsToMove: new List<Guid> { attachment.Id }
        );
        var handler = new SplitTicketCommandHandler(_ticketRepository);

        // Act
        var newTicketId = await handler.Handle(command, CancellationToken.None);
        var newTicket = await _ticketRepository.GetByIdAsync(newTicketId);

        // Assert
        Assert.NotNull(newTicket);
        Assert.Equal(companyId, newTicket.CompanyId);
        Assert.Equal(moduleId, newTicket.ModuleId);
        Assert.Equal("New Subj", newTicket.Subject);

        // Check comment was moved
        Assert.DoesNotContain(originalTicket.TicketComments, c => c.Body == "Move this comment");
        Assert.Single(newTicket.TicketComments, c => c.Body == "Move this comment");

        // Check attachment was moved
        Assert.DoesNotContain(originalTicket.TicketAttachments, a => a.FileName == "file.txt");
        Assert.Single(newTicket.TicketAttachments, a => a.FileName == "file.txt");

        // Check relation is established
        Assert.Contains(originalTicket.TicketRelations, r => r.ChildTicketId == newTicket.Id && r.RelationType == TicketRelationType.SplitFrom);
    }

    [Fact]
    public async Task AddRelation_ShouldThrow_WhenCircularRelationIsCreated()
    {
        var companyId = Guid.NewGuid();
        var moduleId = Guid.NewGuid();
        var ticketA = Ticket.Create(companyId, moduleId, "Subj A", "Desc A");
        var ticketB = Ticket.Create(companyId, moduleId, "Subj B", "Desc B");
        var ticketC = Ticket.Create(companyId, moduleId, "Subj C", "Desc C");

        await _ticketRepository.AddAsync(ticketA);
        await _ticketRepository.AddAsync(ticketB);
        await _ticketRepository.AddAsync(ticketC);

        var handler = new AddRelationCommandHandler(_ticketRepository);

        await handler.Handle(new AddRelationCommand(ticketA.Id, ticketB.Id, TicketRelationType.ParentChild), CancellationToken.None);
        await handler.Handle(new AddRelationCommand(ticketB.Id, ticketC.Id, TicketRelationType.ParentChild), CancellationToken.None);

        await Assert.ThrowsAsync<TicketDomainException>(() =>
            handler.Handle(new AddRelationCommand(ticketC.Id, ticketA.Id, TicketRelationType.ParentChild), CancellationToken.None));
    }

    [Fact]
    public async Task UploadAttachment_ShouldThrow_WhenExtensionIsNotAllowed()
    {
        var companyId = Guid.NewGuid();
        var moduleId = Guid.NewGuid();
        var ticket = Ticket.Create(companyId, moduleId, "Subj", "Desc");
        await _ticketRepository.AddAsync(ticket);

        var fakeFileStorage = new FakeFileStorageService();
        var fakeAttachRepo = new FakeTicketAttachmentRepository();
        var fakeUow = new FakeUnitOfWork();
        var handler = new UploadTicketAttachmentCommandHandler(_ticketRepository, fakeAttachRepo, fakeFileStorage, fakeUow);

        var invalidFiles = new[] { "test.exe", "test.js", "test.bat", "test.cmd", "test.dll", "test.php", "test.cshtml", "test.xls" };
        foreach (var file in invalidFiles)
        {
            var command = new UploadTicketAttachmentCommand(
                ticket.Id,
                null,
                new System.IO.MemoryStream(),
                file,
                12,
                "text/plain",
                Guid.NewGuid()
            );
            await Assert.ThrowsAsync<TicketDomainException>(() => handler.Handle(command, CancellationToken.None));
        }
    }

    [Fact]
    public async Task UploadAttachment_ShouldSucceed_WhenFileIsEmpty()
    {
        var companyId = Guid.NewGuid();
        var moduleId = Guid.NewGuid();
        var ticket = Ticket.Create(companyId, moduleId, "Subj", "Desc");
        await _ticketRepository.AddAsync(ticket);

        var fakeFileStorage = new FakeFileStorageService();
        var fakeAttachRepo = new FakeTicketAttachmentRepository();
        var fakeUow = new FakeUnitOfWork();
        var handler = new UploadTicketAttachmentCommandHandler(_ticketRepository, fakeAttachRepo, fakeFileStorage, fakeUow);

        var uploaderId = Guid.NewGuid();
        _ticketRepository.UserCompanyMap[uploaderId] = companyId; // Valid company

        var command = new UploadTicketAttachmentCommand(
            ticket.Id,
            null,
            new System.IO.MemoryStream(),
            "empty.txt",
            0,
            "text/plain",
            uploaderId
        );

        var attachmentId = await handler.Handle(command, CancellationToken.None);
        Assert.NotEqual(Guid.Empty, attachmentId);
    }

    [Fact]
    public async Task UploadAttachment_ShouldThrow_WhenUploadedByCompanyMismatches()
    {
        var companyId = Guid.NewGuid();
        var moduleId = Guid.NewGuid();
        var ticket = Ticket.Create(companyId, moduleId, "Subj", "Desc");
        await _ticketRepository.AddAsync(ticket);

        var fakeFileStorage = new FakeFileStorageService();
        var fakeAttachRepo = new FakeTicketAttachmentRepository();
        var fakeUow = new FakeUnitOfWork();
        var handler = new UploadTicketAttachmentCommandHandler(_ticketRepository, fakeAttachRepo, fakeFileStorage, fakeUow);

        var uploaderId = Guid.NewGuid();
        _ticketRepository.UserCompanyMap[uploaderId] = Guid.NewGuid(); // Mismatch company

        var command = new UploadTicketAttachmentCommand(
            ticket.Id,
            null,
            new System.IO.MemoryStream(),
            "test.txt",
            12,
            "text/plain",
            uploaderId
        );

        await Assert.ThrowsAsync<TicketDomainException>(() => handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task GetAttachment_ShouldThrow_WhenDownloaderCompanyMismatches()
    {
        var companyId = Guid.NewGuid();
        var moduleId = Guid.NewGuid();
        var ticket = Ticket.Create(companyId, moduleId, "Subj", "Desc");
        await _ticketRepository.AddAsync(ticket);

        var uploaderId = Guid.NewGuid();
        var attachment = TicketAttachment.Create(ticket.Id, null, "test.txt", "http://fakeurl/test.txt", 12, "text/plain", uploaderId);
        ticket.AddAttachment(attachment);

        var fakeAttachRepo = new FakeTicketAttachmentRepository { AttachmentToReturn = attachment };
        var fakeFileStorage = new FakeFileStorageService();
        var downloaderId = Guid.NewGuid();
        var fakeCurrentUserService = new FakeCurrentUserService { UserId = downloaderId };

        _ticketRepository.UserCompanyMap[downloaderId] = Guid.NewGuid(); // Mismatch company

        var handler = new GetAttachmentQueryHandler(fakeAttachRepo, _ticketRepository, fakeFileStorage, fakeCurrentUserService);
        var query = new GetAttachmentQuery(ticket.Id, attachment.Id);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => handler.Handle(query, CancellationToken.None));
    }

    [Fact]
    public async Task AssignAgent_ShouldThrow_WhenAssignerCompanyMismatches()
    {
        var companyId = Guid.NewGuid();
        var moduleId = Guid.NewGuid();
        var ticket = Ticket.Create(companyId, moduleId, "Subj", "Desc");
        await _ticketRepository.AddAsync(ticket);

        var assignerId = Guid.NewGuid();
        _ticketRepository.UserCompanyMap[assignerId] = Guid.NewGuid(); // Mismatch company

        var agentId = Guid.NewGuid();
        _ticketRepository.UserCompanyMap[agentId] = companyId; // Valid company

        var command = new AssignTicketCommand(ticket.Id, agentId, assignerId, "notes");
        var handler = new AssignTicketCommandHandler(_ticketRepository);

        await Assert.ThrowsAsync<TicketDomainException>(() => handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task AssignAgent_ShouldThrow_WhenAgentCompanyMismatches()
    {
        var companyId = Guid.NewGuid();
        var moduleId = Guid.NewGuid();
        var ticket = Ticket.Create(companyId, moduleId, "Subj", "Desc");
        await _ticketRepository.AddAsync(ticket);

        var assignerId = Guid.NewGuid();
        _ticketRepository.UserCompanyMap[assignerId] = companyId; // Valid company

        var agentId = Guid.NewGuid();
        _ticketRepository.UserCompanyMap[agentId] = Guid.NewGuid(); // Mismatch company

        var command = new AssignTicketCommand(ticket.Id, agentId, assignerId, "notes");
        var handler = new AssignTicketCommandHandler(_ticketRepository);

        await Assert.ThrowsAsync<TicketDomainException>(() => handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task MergeTicket_ShouldThrow_WhenMergerCompanyMismatches()
    {
        var companyId = Guid.NewGuid();
        var moduleId = Guid.NewGuid();
        var master = Ticket.Create(companyId, moduleId, "Master", "Desc");
        var duplicate = Ticket.Create(companyId, moduleId, "Duplicate", "Desc");
        await _ticketRepository.AddAsync(master);
        await _ticketRepository.AddAsync(duplicate);

        var mergerId = Guid.NewGuid();
        _ticketRepository.UserCompanyMap[mergerId] = Guid.NewGuid(); // Mismatch company

        var fakeUow = new FakeUnitOfWork();
        var command = new MergeTicketCommand(master.Id, duplicate.Id, mergerId);
        var handler = new MergeTicketCommandHandler(_ticketRepository, fakeUow);

        await Assert.ThrowsAsync<TicketDomainException>(() => handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task SplitTicket_ShouldThrow_WhenSplitterCompanyMismatches()
    {
        var companyId = Guid.NewGuid();
        var moduleId = Guid.NewGuid();
        var originalTicket = Ticket.Create(companyId, moduleId, "Original Subj", "Original Desc");
        await _ticketRepository.AddAsync(originalTicket);

        var splitterId = Guid.NewGuid();
        _ticketRepository.UserCompanyMap[splitterId] = Guid.NewGuid(); // Mismatch company

        var command = new SplitTicketCommand(
            originalTicket.Id,
            "New Subj",
            "New Desc",
            splitterId
        );
        var handler = new SplitTicketCommandHandler(_ticketRepository);

        await Assert.ThrowsAsync<TicketDomainException>(() => handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task SplitTicket_ShouldThrow_WhenCommentDoesNotBelongToOriginal()
    {
        var companyId = Guid.NewGuid();
        var moduleId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var originalTicket = Ticket.Create(companyId, moduleId, "Original Subj", "Original Desc");
        await _ticketRepository.AddAsync(originalTicket);

        _ticketRepository.UserCompanyMap[userId] = companyId; // Valid company

        var command = new SplitTicketCommand(
            originalTicket.Id,
            "New Subj",
            "New Desc",
            userId,
            CommentIdsToMove: new List<Guid> { Guid.NewGuid() }
        );
        var handler = new SplitTicketCommandHandler(_ticketRepository);

        await Assert.ThrowsAsync<TicketDomainException>(() => handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task SplitTicket_ShouldThrow_WhenAttachmentDoesNotBelongToOriginal()
    {
        var companyId = Guid.NewGuid();
        var moduleId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var originalTicket = Ticket.Create(companyId, moduleId, "Original Subj", "Original Desc");
        await _ticketRepository.AddAsync(originalTicket);

        _ticketRepository.UserCompanyMap[userId] = companyId; // Valid company

        var command = new SplitTicketCommand(
            originalTicket.Id,
            "New Subj",
            "New Desc",
            userId,
            AttachmentIdsToMove: new List<Guid> { Guid.NewGuid() }
        );
        var handler = new SplitTicketCommandHandler(_ticketRepository);

        await Assert.ThrowsAsync<TicketDomainException>(() => handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task SplitTicket_ShouldThrow_WhenTicketIsClosed()
    {
        var companyId = Guid.NewGuid();
        var moduleId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var ticket = Ticket.Create(companyId, moduleId, "Original Subj", "Original Desc");
        
        ticket.ChangeStatus(TicketStatus.Open, userId);
        ticket.MarkCustomerCallTaken(userId);
        ticket.ProvideRootCauseAnalysis("Fixed", userId);
        ticket.Resolve(userId, "Resolved");
        ticket.Close(userId, "Closed");
        await _ticketRepository.AddAsync(ticket);

        _ticketRepository.UserCompanyMap[userId] = companyId; // Valid company

        var command = new SplitTicketCommand(
            ticket.Id,
            "New Subj",
            "New Desc",
            userId
        );
        var handler = new SplitTicketCommandHandler(_ticketRepository);

        await Assert.ThrowsAsync<TicketDomainException>(() => handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task AddWatcher_ShouldThrow_WhenWatcherCompanyMismatches()
    {
        var companyId = Guid.NewGuid();
        var moduleId = Guid.NewGuid();
        var ticket = Ticket.Create(companyId, moduleId, "Subj", "Desc");
        await _ticketRepository.AddAsync(ticket);

        var watcherId = Guid.NewGuid();
        _ticketRepository.UserCompanyMap[watcherId] = Guid.NewGuid(); // Mismatch company
        var adderId = Guid.NewGuid();
        _ticketRepository.UserCompanyMap[adderId] = companyId; // Valid company

        var command = new AddWatcherCommand(ticket.Id, watcherId, adderId);
        var handler = new AddWatcherCommandHandler(_ticketRepository);

        await Assert.ThrowsAsync<TicketDomainException>(() => handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task AddWatcher_ShouldThrow_WhenAdderCompanyMismatches()
    {
        var companyId = Guid.NewGuid();
        var moduleId = Guid.NewGuid();
        var ticket = Ticket.Create(companyId, moduleId, "Subj", "Desc");
        await _ticketRepository.AddAsync(ticket);

        var watcherId = Guid.NewGuid();
        _ticketRepository.UserCompanyMap[watcherId] = companyId; // Valid company
        var adderId = Guid.NewGuid();
        _ticketRepository.UserCompanyMap[adderId] = Guid.NewGuid(); // Mismatch company

        var command = new AddWatcherCommand(ticket.Id, watcherId, adderId);
        var handler = new AddWatcherCommandHandler(_ticketRepository);

        await Assert.ThrowsAsync<TicketDomainException>(() => handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public void TicketComment_ShouldThrow_WhenBothAuthorAndContactAreProvided()
    {
        var ticketId = Guid.NewGuid();
        var authorId = Guid.NewGuid();
        var contactId = Guid.NewGuid();

        Assert.Throws<TicketDomainException>(() =>
            TicketComment.Create(ticketId, authorId, contactId, "Test Body", CommentVisibility.Public));
    }

    [Fact]
    public void TicketComment_ShouldThrow_WhenCustomerPostsInternalComment()
    {
        var ticketId = Guid.NewGuid();
        var contactId = Guid.NewGuid();

        Assert.Throws<TicketDomainException>(() =>
            TicketComment.Create(ticketId, null, contactId, "Test Body", CommentVisibility.Internal));
    }

    private class FakeFileStorageService : IFileStorageService
    {
        public Task<string> SaveAsync(System.IO.Stream stream, string fileName, string folder, CancellationToken cancellationToken = default)
            => Task.FromResult("http://fakeurl/" + fileName);

        public Task<System.IO.Stream> OpenReadAsync(string fileUrl, CancellationToken cancellationToken = default)
            => Task.FromResult<System.IO.Stream>(new System.IO.MemoryStream());

        public Task DeleteAsync(string fileUrl, CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task<bool> ExistsAsync(string fileUrl, CancellationToken cancellationToken = default)
            => Task.FromResult(true);
    }

    private class FakeTicketAttachmentRepository : ITicketAttachmentRepository
    {
        public TicketAttachment? AttachmentToReturn { get; set; }
        public Task AddAsync(TicketAttachment attachment, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task<TicketAttachment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) => Task.FromResult(AttachmentToReturn);
        public void Remove(TicketAttachment attachment) { }
    }

    private class FakeCurrentUserService : ICurrentUserService
    {
        public Guid? UserId { get; set; }
        public bool IsAuthenticated => UserId.HasValue;
    }

    private class FakeUnitOfWork : Adrenalin.SharedKernel.Interfaces.IUnitOfWork
    {
        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(1);
        }
    }
}
