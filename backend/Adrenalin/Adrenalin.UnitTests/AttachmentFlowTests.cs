using Adrenalin.Modules.Ticketing.Application.Commands;
using Adrenalin.Modules.Ticketing.Application.Validators;
using System.IO;
using Xunit;

namespace Adrenalin.UnitTests;

public class AttachmentFlowTests
{
    private readonly UploadTicketAttachmentCommandValidator _validator;

    public AttachmentFlowTests()
    {
        _validator = new UploadTicketAttachmentCommandValidator();
    }

    [Theory]
    [InlineData("document.pdf")]
    [InlineData("image.png")]
    [InlineData("archive.zip")]
    [InlineData("notes.txt")]
    [InlineData("document.docx")]
    public void Validator_ShouldPass_ForSafeFileExtensions(string fileName)
    {
        // Arrange
        using var stream = new MemoryStream(new byte[] { 1, 2, 3 });
        var command = new UploadTicketAttachmentCommand(
            TicketId: Guid.NewGuid(),
            CommentId: null,
            Stream: stream,
            FileName: fileName,
            Length: stream.Length,
            ContentType: "application/octet-stream",
            UploadedBy: Guid.NewGuid()
        );

        // Act
        var result = _validator.Validate(command);

        // Assert
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
    public void Validator_ShouldFail_ForUnsafeFileExtensions(string fileName)
    {
        // Arrange
        using var stream = new MemoryStream(new byte[] { 1, 2, 3 });
        var command = new UploadTicketAttachmentCommand(
            TicketId: Guid.NewGuid(),
            CommentId: null,
            Stream: stream,
            FileName: fileName,
            Length: stream.Length,
            ContentType: "application/octet-stream",
            UploadedBy: Guid.NewGuid()
        );

        // Act
        var result = _validator.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        var errors = result.Errors;
        Assert.Contains(errors, e => e.PropertyName == "FileName" && e.ErrorMessage.Contains("security reasons"));
    }

    [Fact]
    public void Validator_ShouldFail_WhenFileSizeExceedsMaxSize()
    {
        // Arrange
        using var stream = new MemoryStream(new byte[] { 1, 2, 3 });
        var command = new UploadTicketAttachmentCommand(
            TicketId: Guid.NewGuid(),
            CommentId: null,
            Stream: stream,
            FileName: "large_file.png",
            Length: 51L * 1024 * 1024, // 51 MB (Limit is 50 MB)
            ContentType: "image/png",
            UploadedBy: Guid.NewGuid()
        );

        // Act
        var result = _validator.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Length");
    }
}
