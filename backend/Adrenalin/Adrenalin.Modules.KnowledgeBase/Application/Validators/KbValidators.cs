using Adrenalin.Modules.KB.Application.Commands;
using Adrenalin.Modules.KB.Domain.ValueObjects;
using FluentValidation;

namespace Adrenalin.Modules.KB.Application.Validators;

// ─── Folder ───────────────────────────────────────────────────────────────────

public sealed class CreateKbFolderCommandValidator : AbstractValidator<CreateKbFolderCommand>
{
    public CreateKbFolderCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Folder name is required.")
            .MaximumLength(FolderName.MaxLength)
            .WithMessage($"Folder name cannot exceed {FolderName.MaxLength} characters.");

        RuleFor(x => x.DisplayOrder)
            .GreaterThanOrEqualTo(0).WithMessage("DisplayOrder must be 0 or greater.");
    }
}

public sealed class RenameKbFolderCommandValidator : AbstractValidator<RenameKbFolderCommand>
{
    public RenameKbFolderCommandValidator()
    {
        RuleFor(x => x.FolderId).NotEmpty();
        RuleFor(x => x.NewName)
            .NotEmpty()
            .MaximumLength(FolderName.MaxLength);
        RuleFor(x => x.ActorId).NotEmpty();
    }
}

public sealed class ReorderKbFolderCommandValidator : AbstractValidator<ReorderKbFolderCommand>
{
    public ReorderKbFolderCommandValidator()
    {
        RuleFor(x => x.FolderId).NotEmpty();
        RuleFor(x => x.NewDisplayOrder).GreaterThanOrEqualTo(0);
        RuleFor(x => x.ActorId).NotEmpty();
    }
}

public sealed class DeleteKbFolderCommandValidator : AbstractValidator<DeleteKbFolderCommand>
{
    public DeleteKbFolderCommandValidator()
    {
        RuleFor(x => x.FolderId).NotEmpty();
        RuleFor(x => x.ActorId).NotEmpty();
    }
}

// ─── Article ──────────────────────────────────────────────────────────────────

public sealed class CreateKbArticleCommandValidator : AbstractValidator<CreateKbArticleCommand>
{
    public CreateKbArticleCommandValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Article title is required.")
            .MaximumLength(ArticleTitle.MaxLength)
            .WithMessage($"Title cannot exceed {ArticleTitle.MaxLength} characters.");

        RuleFor(x => x.ArticleType)
            .IsInEnum().WithMessage("Invalid article type.");
    }
}

public sealed class UpdateKbArticleCommandValidator : AbstractValidator<UpdateKbArticleCommand>
{
    public UpdateKbArticleCommandValidator()
    {
        RuleFor(x => x.ArticleId).NotEmpty();
        RuleFor(x => x.NewTitle)
            .NotEmpty()
            .MaximumLength(ArticleTitle.MaxLength);
    }
}

public sealed class PublishKbArticleCommandValidator : AbstractValidator<PublishKbArticleCommand>
{
    public PublishKbArticleCommandValidator() =>
        RuleFor(x => x.ArticleId).NotEmpty();
}

public sealed class ArchiveKbArticleCommandValidator : AbstractValidator<ArchiveKbArticleCommand>
{
    public ArchiveKbArticleCommandValidator() =>
        RuleFor(x => x.ArticleId).NotEmpty();
}

public sealed class RestoreKbArticleToDraftCommandValidator
    : AbstractValidator<RestoreKbArticleToDraftCommand>
{
    public RestoreKbArticleToDraftCommandValidator() =>
        RuleFor(x => x.ArticleId).NotEmpty();
}

public sealed class DeleteKbArticleCommandValidator : AbstractValidator<DeleteKbArticleCommand>
{
    public DeleteKbArticleCommandValidator() =>
        RuleFor(x => x.ArticleId).NotEmpty();
}

// ─── Auto-resolve ─────────────────────────────────────────────────────────────

public sealed class EnableAutoResolveCommandValidator : AbstractValidator<EnableAutoResolveCommand>
{
    public EnableAutoResolveCommandValidator()
    {
        RuleFor(x => x.ArticleId).NotEmpty();

        RuleFor(x => x.Keywords)
            .NotNull()
            .Must(k => k.Count > 0)
            .WithMessage("At least one keyword is required for auto-resolve.")
            .Must(k => k.All(kw => !string.IsNullOrWhiteSpace(kw)))
            .WithMessage("Keywords must not contain blank entries.")
            .Must(k => k.All(kw => kw.Length <= 100))
            .WithMessage("Each keyword must be 100 characters or fewer.");

        RuleFor(x => x.ResolutionText)
            .NotEmpty().WithMessage("Resolution text is required when enabling auto-resolve.")
            .MaximumLength(5000);

        RuleFor(x => x.ConfidenceThreshold)
            .InclusiveBetween(ConfidenceThreshold.Minimum,ConfidenceThreshold.Maximum)
            .WithMessage($"Confidence threshold must be between " +
                         $"{ConfidenceThreshold.Minimum} and {ConfidenceThreshold.Maximum}.");
    }
}

public sealed class RecordArticleReopenedMatchCommandValidator
    : AbstractValidator<RecordArticleReopenedMatchCommand>
{
    public RecordArticleReopenedMatchCommandValidator()
    {
        RuleFor(x => x.ArticleId).NotEmpty();
        RuleFor(x => x.ThresholdRaiseDelta)
            .GreaterThan(0m).WithMessage("Delta must be positive.")
            .LessThanOrEqualTo(0.1m).WithMessage("Delta cannot exceed 0.1 per step.");
    }
}

// ─── Attachment ───────────────────────────────────────────────────────────────

public sealed class AddAttachmentCommandValidator : AbstractValidator<AddAttachmentCommand>
{
    private static readonly string[] AllowedMimeTypes =
    [
        "application/pdf",
        "image/png", "image/jpeg", "image/gif", "image/webp",
        "text/plain", "text/csv",
        "application/vnd.ms-excel",
        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        "application/msword",
        "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
        "application/zip"
    ];

    public AddAttachmentCommandValidator()
    {
        RuleFor(x => x.ArticleId).NotEmpty();

        RuleFor(x => x.FileName)
            .NotEmpty()
            .MaximumLength(255);

        RuleFor(x => x.FileStream)
            .NotNull().WithMessage("File content stream is required.");

        RuleFor(x => x.FileSizeBytes)
            .GreaterThan(0L).When(x => x.FileSizeBytes.HasValue)
            .LessThanOrEqualTo(52_428_800L).When(x => x.FileSizeBytes.HasValue)
            .WithMessage("File size cannot exceed 50 MB.");

        RuleFor(x => x.MimeType)
            .Must(m => m == null || AllowedMimeTypes.Contains(m))
            .WithMessage("MIME type is not in the allowed list.");
    }
}

public sealed class RemoveAttachmentCommandValidator : AbstractValidator<RemoveAttachmentCommand>
{
    public RemoveAttachmentCommandValidator()
    {
        RuleFor(x => x.ArticleId).NotEmpty();
        RuleFor(x => x.AttachmentId).NotEmpty();
    }
}

// ─── Portal Banner ────────────────────────────────────────────────────────────

public sealed class CreatePortalBannerCommandValidator : AbstractValidator<CreatePortalBannerCommand>
{
    public CreatePortalBannerCommandValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Message).NotEmpty();
        RuleFor(x => x)
            .Must(x => x.ActiveFrom == null || x.ActiveTo == null || x.ActiveTo > x.ActiveFrom)
            .WithMessage("active_to must be after active_from.");
    }
}

public sealed class UpdatePortalBannerCommandValidator : AbstractValidator<UpdatePortalBannerCommand>
{
    public UpdatePortalBannerCommandValidator()
    {
        RuleFor(x => x.BannerId).NotEmpty();
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Message).NotEmpty();
        RuleFor(x => x)
            .Must(x => x.ActiveFrom == null || x.ActiveTo == null || x.ActiveTo > x.ActiveFrom)
            .WithMessage("active_to must be after active_from.");
    }
}
