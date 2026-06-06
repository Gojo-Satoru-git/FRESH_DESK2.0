using Adrenalin.Modules.Auth.Domain.Interfaces;
using Adrenalin.Modules.Ticketing.Domain.Interfaces;
using Adrenalin.Persistence.Context;
using Adrenalin.Persistence.Repositories;
using Adrenalin.Persistence.Repositories.KnowledgeBase;
using Adrenalin.SharedKernel.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Adrenalin.Persistence;

public static class PersistenceServiceCollectionExtensions
{
    public static IServiceCollection AddPersistence(this IServiceCollection services)
    {
        // UnitOfWork — powered by the single consolidated context
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<AdrenalinDbContext>());

        // Auth
        services.AddScoped<IUserRepository, UserRepository>();

        // Ticketing
        services.AddScoped<ITicketRepository, TicketRepository>();
        services.AddScoped<ITicketAttachmentRepository, TicketAttachmentRepository>();

        // KnowledgeBase
        services.AddScoped<Adrenalin.Modules.KB.Domain.Interfaces.IKbArticleRepository, KbArticleRepository>();
        services.AddScoped<Adrenalin.Modules.KB.Domain.Interfaces.IKbFolderRepository, KbFolderRepository>();
        services.AddScoped<Adrenalin.Modules.KB.Domain.Interfaces.IPortalBannerRepository, PortalBannerRepository>();

        return services;
    }
}