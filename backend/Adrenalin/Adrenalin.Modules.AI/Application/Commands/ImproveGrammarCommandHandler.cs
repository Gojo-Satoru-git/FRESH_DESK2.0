using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Adrenalin.Modules.AI.Application.Contracts;

namespace Adrenalin.Modules.AI.Application.Commands;

internal sealed class ImproveGrammarCommandHandler : IRequestHandler<ImproveGrammarCommand, string>
{
    private readonly IAiProvider _aiProvider;

    public ImproveGrammarCommandHandler(IAiProvider aiProvider)
    {
        _aiProvider = aiProvider;
    }

    public Task<string> Handle(ImproveGrammarCommand request, CancellationToken cancellationToken)
    {
        return _aiProvider.ImproveGrammarAsync(request.Content, cancellationToken);
    }
}
