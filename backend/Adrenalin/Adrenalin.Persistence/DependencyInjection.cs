using Adrenalin.Modules.Auth.Domain.Interfaces;
using Adrenalin.Modules.Ticketing.Domain.Interfaces;
using Adrenalin.Persistence.Context;
using Adrenalin.Persistence.Repositories;
using Adrenalin.Persistence.Repositories.Auth;
using Adrenalin.Persistence.Repositories.KnowledgeBase;
using Adrenalin.SharedKernel.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Adrenalin.Persistence;

public static class PersistenceServiceCollectionExtensions
{
    public static IServiceCollection AddPersistence(this IServiceCollection services)
    {
        // UnitOfWork
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<AdrenalinDbContext>());

        // ── Auth / RBAC ───────────────────────────────────────────────────────
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<IPermissionRepository, PermissionRepository>();
        services.AddScoped<IRolePermissionRepository, RolePermissionRepository>();
        services.AddScoped<IUserRoleRepository, UserRoleRepository>();
        services.AddScoped<IGroupRepository, GroupRepository>();
        services.AddScoped<IUserGroupRepository, UserGroupRepository>();

        // ── Ticketing ─────────────────────────────────────────────────────────
        services.AddScoped<ITicketRepository, TicketRepository>();
        services.AddScoped<ITicketAttachmentRepository, TicketAttachmentRepository>();
        services.AddScoped<Adrenalin.Modules.Ticketing.Application.Queries.ITicketQueryService, TicketQueryService>();

        // ── KnowledgeBase ─────────────────────────────────────────────────────
        services.AddScoped<Adrenalin.Modules.KB.Domain.Interfaces.IKbArticleRepository, KbArticleRepository>();
        services.AddScoped<Adrenalin.Modules.KB.Domain.Interfaces.IKbFolderRepository, KbFolderRepository>();
        services.AddScoped<Adrenalin.Modules.KB.Domain.Interfaces.IPortalBannerRepository, PortalBannerRepository>();

        // ── Notification ──────────────────────────────────────────────────────
        services.AddScoped<Adrenalin.Modules.Notification.Domain.Interfaces.INotificationRepository, NotificationRepository>();

        return services;
    }
}