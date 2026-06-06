using Adrenalin.Modules.KB.Application.Commands;
using Adrenalin.Modules.KB.Application.Validators;
using Adrenalin.Modules.KB.Domain.Enums;
using Adrenalin.Modules.KB.Domain.ValueObjects;

namespace Adrenalin.UnitTests.KnowledgeBase.Application.Validators;

// ─── Folder Validators ────────────────────────────────────────────────────────

public sealed class CreateKbFolderCommandValidatorTests
{
    private readonly CreateKbFolderCommandValidator _v = new();

    [Fact]
    public void Valid_PassesValidation()
        => Assert.True(_v.Validate(new CreateKbFolderCommand("Support", null, 0, Guid.NewGuid())).IsValid);

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void EmptyName_FailsValidation(string name)
    {
        var r = _v.Validate(new CreateKbFolderCommand(name, null, 0, Guid.NewGuid()));
        Assert.False(r.IsValid);
        Assert.Contains(r.Errors, e => e.PropertyName == "Name");
    }

    [Fact]
    public void NameExceedsMaxLength_FailsValidation()
    {
        var r = _v.Validate(new CreateKbFolderCommand(new string('A', FolderName.MaxLength + 1), null, 0, Guid.NewGuid()));
        Assert.False(r.IsValid);
        Assert.Contains(r.Errors, e => e.PropertyName == "Name");
    }

    [Fact]
    public void NegativeDisplayOrder_FailsValidation()
    {
        var r = _v.Validate(new CreateKbFolderCommand("Name", null, -1, Guid.NewGuid()));
        Assert.False(r.IsValid);
        Assert.Contains(r.Errors, e => e.PropertyName == "DisplayOrder");
    }

    [Fact]
    public void ZeroDisplayOrder_Passes()
        => Assert.True(_v.Validate(new CreateKbFolderCommand("Name", null, 0, Guid.NewGuid())).IsValid);
}

public sealed class RenameKbFolderCommandValidatorTests
{
    private readonly RenameKbFolderCommandValidator _v = new();

    [Fact]
    public void Valid_Passes()
        => Assert.True(_v.Validate(new RenameKbFolderCommand(Guid.NewGuid(), "New", Guid.NewGuid())).IsValid);

    [Fact]
    public void EmptyFolderId_Fails()
        => Assert.False(_v.Validate(new RenameKbFolderCommand(Guid.Empty, "New", Guid.NewGuid())).IsValid);

    [Fact]
    public void EmptyNewName_Fails()
        => Assert.False(_v.Validate(new RenameKbFolderCommand(Guid.NewGuid(), "", Guid.NewGuid())).IsValid);

    [Fact]
    public void EmptyActorId_Fails()
        => Assert.False(_v.Validate(new RenameKbFolderCommand(Guid.NewGuid(), "X", Guid.Empty)).IsValid);
}

public sealed class ReorderKbFolderCommandValidatorTests
{
    private readonly ReorderKbFolderCommandValidator _v = new();

    [Fact]
    public void Valid_Passes()
        => Assert.True(_v.Validate(new ReorderKbFolderCommand(Guid.NewGuid(), 3, Guid.NewGuid())).IsValid);

    [Fact]
    public void NegativeDisplayOrder_Fails()
        => Assert.False(_v.Validate(new ReorderKbFolderCommand(Guid.NewGuid(), -1, Guid.NewGuid())).IsValid);

    [Fact]
    public void EmptyFolderId_Fails()
        => Assert.False(_v.Validate(new ReorderKbFolderCommand(Guid.Empty, 0, Guid.NewGuid())).IsValid);
}

public sealed class DeleteKbFolderCommandValidatorTests
{
    private readonly DeleteKbFolderCommandValidator _v = new();

    [Fact]
    public void Valid_Passes()
        => Assert.True(_v.Validate(new DeleteKbFolderCommand(Guid.NewGuid(), Guid.NewGuid())).IsValid);

    [Fact]
    public void EmptyFolderId_Fails()
        => Assert.False(_v.Validate(new DeleteKbFolderCommand(Guid.Empty, Guid.NewGuid())).IsValid);

    [Fact]
    public void EmptyActorId_Fails()
        => Assert.False(_v.Validate(new DeleteKbFolderCommand(Guid.NewGuid(), Guid.Empty)).IsValid);
}

// ─── Article Validators ───────────────────────────────────────────────────────

public sealed class CreateKbArticleCommandValidatorTests
{
    private readonly CreateKbArticleCommandValidator _v = new();

    [Fact]
    public void Valid_Passes()
        => Assert.True(_v.Validate(
            new CreateKbArticleCommand("Title", null, ArticleType.Faq, null, Guid.NewGuid())).IsValid);

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void EmptyTitle_Fails(string title)
        => Assert.False(_v.Validate(
            new CreateKbArticleCommand(title, null, ArticleType.Faq, null, Guid.NewGuid())).IsValid);

    [Fact]
    public void TitleExceedsMaxLength_Fails()
    {
        var r = _v.Validate(new CreateKbArticleCommand(
            new string('A', ArticleTitle.MaxLength + 1), null, ArticleType.Faq, null, Guid.NewGuid()));
        Assert.False(r.IsValid);
        Assert.Contains(r.Errors, e => e.PropertyName == "Title");
    }

    [Fact]
    public void InvalidArticleType_Fails()
        => Assert.False(_v.Validate(
            new CreateKbArticleCommand("T", null, (ArticleType)99, null, Guid.NewGuid())).IsValid);
}

public sealed class UpdateKbArticleCommandValidatorTests
{
    private readonly UpdateKbArticleCommandValidator _v = new();

    [Fact]
    public void Valid_Passes()
        => Assert.True(_v.Validate(
            new UpdateKbArticleCommand(Guid.NewGuid(), "New Title", null, Guid.NewGuid())).IsValid);

    [Fact]
    public void EmptyArticleId_Fails()
        => Assert.False(_v.Validate(
            new UpdateKbArticleCommand(Guid.Empty, "T", null, Guid.NewGuid())).IsValid);

    [Fact]
    public void EmptyTitle_Fails()
        => Assert.False(_v.Validate(
            new UpdateKbArticleCommand(Guid.NewGuid(), "", null, Guid.NewGuid())).IsValid);

    [Fact]
    public void TitleExceedsMaxLength_Fails()
        => Assert.False(_v.Validate(
            new UpdateKbArticleCommand(Guid.NewGuid(), new string('X', ArticleTitle.MaxLength + 1), null, Guid.NewGuid())).IsValid);
}

// ─── Auto-resolve Validators ──────────────────────────────────────────────────

public sealed class EnableAutoResolveCommandValidatorTests
{
    private readonly EnableAutoResolveCommandValidator _v = new();

    [Fact]
    public void Valid_Passes()
        => Assert.True(_v.Validate(new EnableAutoResolveCommand(
            Guid.NewGuid(), ["login", "password"], "Try resetting your password.", 0.75m, Guid.NewGuid())).IsValid);

    [Fact]
    public void EmptyKeywords_Fails()
    {
        var r = _v.Validate(new EnableAutoResolveCommand(Guid.NewGuid(), [], "text", 0.75m, Guid.NewGuid()));
        Assert.False(r.IsValid);
        Assert.Contains(r.Errors, e => e.PropertyName == "Keywords");
    }

    [Fact]
    public void BlankKeywordEntry_Fails()
        => Assert.False(_v.Validate(new EnableAutoResolveCommand(
            Guid.NewGuid(), ["ok", "   "], "text", 0.75m, Guid.NewGuid())).IsValid);

    [Fact]
    public void KeywordExceeds100Chars_Fails()
        => Assert.False(_v.Validate(new EnableAutoResolveCommand(
            Guid.NewGuid(), [new string('k', 101)], "text", 0.75m, Guid.NewGuid())).IsValid);

    [Fact]
    public void EmptyResolutionText_Fails()
    {
        var r = _v.Validate(new EnableAutoResolveCommand(
            Guid.NewGuid(), ["kw"], "", 0.75m, Guid.NewGuid()));
        Assert.False(r.IsValid);
        Assert.Contains(r.Errors, e => e.PropertyName == "ResolutionText");
    }

    [Theory]
    [InlineData((double)ConfidenceThreshold.Minimum - 0.01)]
    [InlineData((double)ConfidenceThreshold.Maximum + 0.01)]
    public void OutOfRangeThreshold_Fails(double val)
        => Assert.False(_v.Validate(new EnableAutoResolveCommand(
            Guid.NewGuid(), ["kw"], "text", (decimal)val, Guid.NewGuid())).IsValid);

    [Theory]
    [InlineData((double)ConfidenceThreshold.Minimum)]
    [InlineData((double)ConfidenceThreshold.Maximum)]
    [InlineData(0.75)]
    public void BoundaryThresholds_Pass(double val)
        => Assert.True(_v.Validate(new EnableAutoResolveCommand(
            Guid.NewGuid(), ["kw"], "text", (decimal)val, Guid.NewGuid())).IsValid);
}

public sealed class RecordArticleReopenedMatchCommandValidatorTests
{
    private readonly RecordArticleReopenedMatchCommandValidator _v = new();

    [Fact]
    public void Valid_Passes()
        => Assert.True(_v.Validate(new RecordArticleReopenedMatchCommand(Guid.NewGuid(), 0.025m)).IsValid);

    [Fact]
    public void ZeroDelta_Fails()
        => Assert.False(_v.Validate(new RecordArticleReopenedMatchCommand(Guid.NewGuid(), 0m)).IsValid);

    [Fact]
    public void NegativeDelta_Fails()
        => Assert.False(_v.Validate(new RecordArticleReopenedMatchCommand(Guid.NewGuid(), -0.01m)).IsValid);

    [Fact]
    public void DeltaExceedsMax_Fails()
        => Assert.False(_v.Validate(new RecordArticleReopenedMatchCommand(Guid.NewGuid(), 0.11m)).IsValid);

    [Fact]
    public void MaxAllowedDelta_Passes()
        => Assert.True(_v.Validate(new RecordArticleReopenedMatchCommand(Guid.NewGuid(), 0.1m)).IsValid);

    [Fact]
    public void EmptyArticleId_Fails()
        => Assert.False(_v.Validate(new RecordArticleReopenedMatchCommand(Guid.Empty, 0.025m)).IsValid);
}

// ─── Attachment Validators ────────────────────────────────────────────────────

public sealed class AddAttachmentCommandValidatorTests
{
    private readonly AddAttachmentCommandValidator _v = new();

    private static AddAttachmentCommand Valid(
        string fileName = "file.pdf",
        long? size = 2048L,
        string? mime = "application/pdf")
        => new(Guid.NewGuid(), fileName, size, mime, new MemoryStream([1, 2, 3]));

    [Fact]
    public void Valid_Passes() => Assert.True(_v.Validate(Valid()).IsValid);

    [Fact]
    public void EmptyArticleId_Fails()
    {
        var cmd = new AddAttachmentCommand(Guid.Empty, "f.pdf", null, null, new MemoryStream());
        Assert.False(_v.Validate(cmd).IsValid);
    }

    [Fact]
    public void EmptyFileName_Fails()
    {
        var cmd = new AddAttachmentCommand(Guid.NewGuid(), "", null, null, new MemoryStream());
        Assert.False(_v.Validate(cmd).IsValid);
    }

    [Fact]
    public void FileNameTooLong_Fails()
    {
        var cmd = new AddAttachmentCommand(Guid.NewGuid(), new string('f', 256), null, null, new MemoryStream());
        Assert.False(_v.Validate(cmd).IsValid);
    }

    [Fact]
    public void NullFileStream_Fails()
    {
        var cmd = new AddAttachmentCommand(Guid.NewGuid(), "f.pdf", null, null, null!);
        var r = _v.Validate(cmd);
        Assert.False(r.IsValid);
        Assert.Contains(r.Errors, e => e.PropertyName == "FileStream");
    }

    [Fact]
    public void FileSizeExceeds50MB_Fails()
    {
        var cmd = Valid(size: 52_428_801L);
        Assert.False(_v.Validate(cmd).IsValid);
    }

    [Fact]
    public void FileSizeExactly50MB_Passes()
    {
        var cmd = Valid(size: 52_428_800L);
        Assert.True(_v.Validate(cmd).IsValid);
    }

    [Fact]
    public void ZeroFileSize_Fails()
    {
        var cmd = Valid(size: 0L);
        Assert.False(_v.Validate(cmd).IsValid);
    }

    [Fact]
    public void NullFileSize_Passes()
    {
        var cmd = Valid(size: null);
        Assert.True(_v.Validate(cmd).IsValid);
    }

    [Fact]
    public void DisallowedMimeType_Fails()
    {
        var cmd = Valid(mime: "application/x-msdownload");
        var r = _v.Validate(cmd);
        Assert.False(r.IsValid);
        Assert.Contains(r.Errors, e => e.PropertyName == "MimeType");
    }

    [Theory]
    [InlineData("application/pdf")]
    [InlineData("image/png")]
    [InlineData("text/csv")]
    [InlineData("application/zip")]
    public void AllowedMimeTypes_Pass(string mime)
        => Assert.True(_v.Validate(Valid(mime: mime)).IsValid);

    [Fact]
    public void NullMimeType_Passes()
        => Assert.True(_v.Validate(Valid(mime: null)).IsValid);
}

public sealed class RemoveAttachmentCommandValidatorTests
{
    private readonly RemoveAttachmentCommandValidator _v = new();

    [Fact]
    public void Valid_Passes()
        => Assert.True(_v.Validate(new RemoveAttachmentCommand(Guid.NewGuid(), Guid.NewGuid())).IsValid);

    [Fact]
    public void EmptyArticleId_Fails()
        => Assert.False(_v.Validate(new RemoveAttachmentCommand(Guid.Empty, Guid.NewGuid())).IsValid);

    [Fact]
    public void EmptyAttachmentId_Fails()
        => Assert.False(_v.Validate(new RemoveAttachmentCommand(Guid.NewGuid(), Guid.Empty)).IsValid);
}

// ─── Portal Banner Validators ─────────────────────────────────────────────────

public sealed class CreatePortalBannerCommandValidatorTests
{
    private readonly CreatePortalBannerCommandValidator _v = new();

    [Fact]
    public void Valid_NoSchedule_Passes()
        => Assert.True(_v.Validate(new CreatePortalBannerCommand("Title", "Message", null, null, Guid.NewGuid())).IsValid);

    [Fact]
    public void Valid_WithSchedule_Passes()
    {
        var now = DateTimeOffset.UtcNow;
        Assert.True(_v.Validate(new CreatePortalBannerCommand(
            "T", "M", now.AddHours(1), now.AddHours(5), Guid.NewGuid())).IsValid);
    }

    [Fact]
    public void EmptyTitle_Fails()
        => Assert.False(_v.Validate(new CreatePortalBannerCommand("", "M", null, null, Guid.NewGuid())).IsValid);

    [Fact]
    public void TitleExceeds200Chars_Fails()
        => Assert.False(_v.Validate(new CreatePortalBannerCommand(
            new string('T', 201), "M", null, null, Guid.NewGuid())).IsValid);

    [Fact]
    public void EmptyMessage_Fails()
        => Assert.False(_v.Validate(new CreatePortalBannerCommand("T", "", null, null, Guid.NewGuid())).IsValid);

    [Fact]
    public void ActiveToBeforeActiveFrom_Fails()
    {
        var now = DateTimeOffset.UtcNow;
        var r = _v.Validate(new CreatePortalBannerCommand(
            "T", "M", now.AddHours(5), now.AddHours(1), Guid.NewGuid()));
        Assert.False(r.IsValid);
        Assert.Contains("active_to", r.Errors[0].ErrorMessage);
    }

    [Fact]
    public void ActiveToEqualsActiveFrom_Fails()
    {
        var ts = DateTimeOffset.UtcNow.AddHours(3);
        Assert.False(_v.Validate(new CreatePortalBannerCommand("T", "M", ts, ts, Guid.NewGuid())).IsValid);
    }
}

public sealed class UpdatePortalBannerCommandValidatorTests
{
    private readonly UpdatePortalBannerCommandValidator _v = new();

    [Fact]
    public void Valid_Passes()
        => Assert.True(_v.Validate(
            new UpdatePortalBannerCommand(Guid.NewGuid(), "T", "M", null, null, Guid.NewGuid())).IsValid);

    [Fact]
    public void EmptyBannerId_Fails()
        => Assert.False(_v.Validate(
            new UpdatePortalBannerCommand(Guid.Empty, "T", "M", null, null, Guid.NewGuid())).IsValid);

    [Fact]
    public void ActiveToBeforeActiveFrom_Fails()
    {
        var now = DateTimeOffset.UtcNow;
        Assert.False(_v.Validate(new UpdatePortalBannerCommand(
            Guid.NewGuid(), "T", "M", now.AddHours(3), now.AddHours(1), Guid.NewGuid())).IsValid);
    }
}