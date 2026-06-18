using System;
using Adrenalin.SharedKernel.Mediator;
using Adrenalin.SharedKernel.Results;
using System.Collections.Generic;

namespace Adrenalin.Modules.Ticketing.Application.Commands.Tickets;

public sealed record CreateTicketFromEmailCommand(
    Guid CompanyId,
    Guid? ContactId,
    Guid ModuleId,
    Guid EmailMessageId,
    string Subject,
    string PlainTextBody,
    string? HtmlBody,
    IReadOnlyList<Guid>? WatcherUserIds,
    Guid? CreatedByUserId,
    IReadOnlyList<Adrenalin.EventBus.Events.EmailAttachmentDto>? Attachments = null
) : IRequest<Result<Guid>>;
