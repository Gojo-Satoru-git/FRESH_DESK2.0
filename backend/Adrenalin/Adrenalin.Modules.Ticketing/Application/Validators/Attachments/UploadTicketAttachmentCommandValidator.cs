using Adrenalin.Modules.Ticketing.Application.Commands;
using FluentValidation;
using System.IO;
using System.Linq;

namespace Adrenalin.Modules.Ticketing.Application.Validators;

public sealed class UploadTicketAttachmentCommandValidator : AbstractValidator<UploadTicketAttachmentCommand>
{
    private const long MaxSize = 50 * 1024 * 1024;

    private static readonly string[] AllowedExtensions =
    {
        ".pdf", ".doc", ".docx", ".xlsx",
        ".csv", ".png", ".jpg", ".jpeg", ".txt", ".mp4"
    };

    public UploadTicketAttachmentCommandValidator()
    {
        RuleFor(x => x.TicketId).NotEmpty();

        RuleFor(x => x.UploadedBy).NotEmpty();

        RuleFor(x => x.Stream).NotNull();

        RuleFor(x => x.FileName)
            .NotEmpty()
            .Must(fileName => AllowedExtensions
            .Contains(Path.GetExtension(fileName).ToLowerInvariant()))
            .WithMessage("File extension is not allowed for security reasons.");

        RuleFor(x => x.Length).GreaterThanOrEqualTo(0).LessThanOrEqualTo(MaxSize);

        RuleFor(x => x.ContentType).NotEmpty();
    }
}