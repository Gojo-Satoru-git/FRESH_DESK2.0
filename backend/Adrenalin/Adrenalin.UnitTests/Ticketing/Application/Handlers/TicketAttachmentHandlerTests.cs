using Adrenalin.Modules.Ticketing.Application.Commands;
using Adrenalin.Modules.Ticketing.Application.Handlers;
using Adrenalin.Modules.Ticketing.Application.Queries;
using Adrenalin.Modules.Ticketing.Application.Validators;
using Adrenalin.Modules.Ticketing.Domain.Entities;
using Adrenalin.Modules.Ticketing.Domain.Enums;
using Adrenalin.UnitTests.Fakes;

namespace Adrenalin.UnitTests.Ticketing.Application.Handlers;

// Tests for ticket attachment commands and queries:
// Upload (validator + handler), Download (company mismatch guard).
public class TicketAttachmentHandlerTests
{
    private readonly FakeTicketRepository _repo;
    private readonly UploadTicketAttachmentCommandValidator _validator;

    public TicketAttachmentHandlerTests()
    {
        _repo      = new FakeTicketRepository();
        _validator = new UploadTicketAttachmentCommandValidator();
    }

    // Validator — allowed extensions
    [Theory]
    [InlineData("document.pdf")]
    [InlineData("image.png")]
    [InlineData("archive.zip")]
    [InlineData("notes.txt")]
    [InlineData("document.docx")]
    public void Validator_ShouldPass_ForSafeExtensions(string fileName)
    {
        using var stream  = new MemoryStream([1, 2, 3]);
        var command       = BuildUploadCommand(fileName, stream, stream.Length);
        var result        = _validator.Validate(command);
        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("script.js")]
    [InlineData("index.html")]
    [InlineData("malware.exe")]
    [InlineData("payload.cshtml")]
    [InlineData("backdoor.php")]
    [InlineData("setup.msi")]
    [InlineData("presentation.pptx")]
    public void Validator_ShouldFail_ForUnsafeExtensions(string fileName)
    {
        using var stream = new MemoryStream([1, 2, 3]);
        var command      = BuildUploadCommand(fileName, stream, stream.Length);
        var result       = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e =>
            e.PropertyName == "FileName" && e.ErrorMessage.Contains("security reasons"));
    }

    [Fact]
    public void Validator_ShouldFail_WhenFileSizeExceedsLimit()
    {
        using var stream = new MemoryStream([1, 2, 3]);
        var command      = BuildUploadCommand("large.png", stream, 51L * 1024 * 1024); // > 50 MB

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Length");
    }

    // Upload handler
    [Fact]
    public async Task Upload_ShouldSucceed_WhenFileIsEmpty()
    {
        // Arrange
        var companyId = Guid.NewGuid();
        var ticket    = Ticket.Create(companyId, Guid.NewGuid(), "Subj", "Desc");
        await _repo.AddAsync(ticket);

        var uploaderId = Guid.NewGuid();
        _repo.UserCompanyMap[uploaderId] = companyId;

        var command = BuildUploadCommand("empty.txt", new MemoryStream(), 0, ticket.Id, uploaderId);
        var handler = BuildUploadHandler();

        // Act
        var attachmentId = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotEqual(Guid.Empty, attachmentId);
    }

    [Fact]
    public async Task Upload_ShouldThrow_WhenExtensionIsBlocked()
    {
        // Arrange
        var ticket = Ticket.Create(Guid.NewGuid(), Guid.NewGuid(), "Subj", "Desc");
        await _repo.AddAsync(ticket);
        var handler = BuildUploadHandler();

        foreach (var blocked in new[] { "test.exe", "test.js", "test.bat", "test.dll", "test.php" })
        {
            var command = BuildUploadCommand(blocked, new MemoryStream(), 12, ticket.Id);
            await Assert.ThrowsAsync<TicketDomainException>(() =>
                handler.Handle(command, CancellationToken.None));
        }
    }

    [Fact]
    public async Task Upload_ShouldThrow_WhenUploaderBelongsToDifferentCompany()
    {
        // Arrange
        var companyId = Guid.NewGuid();
        var ticket    = Ticket.Create(companyId, Guid.NewGuid(), "Subj", "Desc");
        await _repo.AddAsync(ticket);

        var uploaderId = Guid.NewGuid();
        _repo.UserCompanyMap[uploaderId] = Guid.NewGuid(); // wrong company

        var command = BuildUploadCommand("test.txt", new MemoryStream(), 12, ticket.Id, uploaderId);
        var handler = BuildUploadHandler();

        // Act & Assert
        await Assert.ThrowsAsync<TicketDomainException>(() =>
            handler.Handle(command, CancellationToken.None));
    }

    // Download handler 

    [Fact]
    public async Task GetAttachment_ShouldThrow_WhenDownloaderBelongsToDifferentCompany()
    {
        // Arrange
        var companyId  = Guid.NewGuid();
        var ticket     = Ticket.Create(companyId, Guid.NewGuid(), "Subj", "Desc");
        var uploaderId = Guid.NewGuid();
        var attachment = TicketAttachment.Create(ticket.Id, null, "test.txt", "http://fakeurl/test.txt", 12, "text/plain", uploaderId);
        ticket.AddAttachment(attachment);
        await _repo.AddAsync(ticket);

        var downloaderId = Guid.NewGuid();
        _repo.UserCompanyMap[downloaderId] = Guid.NewGuid(); // wrong company

        var attachRepo          = new FakeTicketAttachmentRepository { AttachmentToReturn = attachment };
        var currentUserService  = new FakeCurrentUserService { UserId = downloaderId };
        var handler             = new GetAttachmentQueryHandler(attachRepo, _repo, new FakeFileStorageService(), currentUserService);
        var query               = new GetAttachmentQuery(ticket.Id, attachment.Id);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            handler.Handle(query, CancellationToken.None));
    }

    // Helpers
    private static UploadTicketAttachmentCommand BuildUploadCommand(
        string fileName,
        Stream stream,
        long length,
        Guid? ticketId   = null,
        Guid? uploadedBy = null) =>
        new(
            TicketId:    ticketId   ?? Guid.NewGuid(),
            CommentId:   null,
            Stream:      stream,
            FileName:    fileName,
            Length:      length,
            ContentType: "application/octet-stream",
            UploadedBy:  uploadedBy ?? Guid.NewGuid()
        );

    private UploadTicketAttachmentCommandHandler BuildUploadHandler() =>
        new(_repo, new FakeTicketAttachmentRepository(), new FakeFileStorageService(), new FakeUnitOfWork());
}
