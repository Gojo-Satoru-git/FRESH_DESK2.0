using Adrenalin.SharedKernel.Mediator;
using Adrenalin.SharedKernel.Results;

namespace Adrenalin.Modules.SLA.Application.Commands;

public record CheckEscalationsCommand()
    : IRequest<Result<int>>;