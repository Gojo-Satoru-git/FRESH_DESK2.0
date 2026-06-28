using Adrenalin.Modules.Auth.Domain.Interfaces;
using Adrenalin.Modules.Company.Domain.Interfaces;
using Adrenalin.Modules.Notification.Application.IntegrationEvents;
using Adrenalin.Modules.SLA.Domain.Interfaces;
using Adrenalin.Modules.Ticketing.Domain.Interfaces;
using Adrenalin.Persistence.Context;
using Adrenalin.Persistence.Repositories;
using Adrenalin.Persistence.Repositories.Auth;
using Adrenalin.Persistence.Repositories.KnowledgeBase;
using Adrenalin.Persistence.Repositories.SLA;
using Adrenalin.Persistence.Repositories.Workflow;
using Adrenalin.SharedKernel.Contracts;
using Adrenalin.SharedKernel.Interfaces;
using Adrenalin.SharedKernel.Mediator;
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

        services.AddScoped<IWorkflowRoleRepository, WorkflowRoleRepository>();
        services.AddScoped<IUserWorkflowRoleRepository, UserWorkflowRoleRepository>();
        services.AddScoped<IAuditLogWriter, AuditLogWriter>();
        services.AddScoped<IStageRoleReferenceChecker, NotBuiltYetStageRoleReferenceChecker>();

        // ── SLA ─────────────────────────────────────────────────────────

        services.AddScoped<ISlaRepository, SlaRepository>();

        // ── Ticketing ─────────────────────────────────────────────────────────
        services.AddScoped<ITicketRepository, TicketRepository>();
        services.AddScoped<ITicketDashboardRepository, TicketDashboardRepository>();
        services.AddScoped<ITicketRoutingContextRepository, TicketRoutingContextRepository>();

        // ── Ticketing Email Repositories ──────────────────────────────────────
        services.AddScoped<IEmailMessageRepository, EmailMessageRepository>();
        services.AddScoped<IProcessedEmailLogRepository, ProcessedEmailLogRepository>();
        services.AddScoped<IEmailAliasRoutingRepository, EmailAliasRoutingRepository>();

        services.AddScoped<IAutomationRuleRepository, AutomationRuleRepository>();

        services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<AdrenalinDbContext>());

        services.AddScoped<ITicketAttachmentRepository, TicketAttachmentRepository>();
        services.AddScoped<ITicketVisibilityService, TicketVisibilityService>();
        services.AddScoped<Adrenalin.Modules.Ticketing.Application.Queries.ITicketQueryService, TicketQueryService>();
        services.AddScoped<Adrenalin.Modules.Lookup.Application.Queries.ILookupQueryService, LookupQueryService>();

        // ── Company ───────────────────────────────────────────────────────────
        services.AddScoped<ICompanyRepository, CompanyRepository>();
        services.AddScoped<IContactRepository, ContactRepository>();
        services.AddScoped<Adrenalin.Modules.Company.Application.Queries.ICompanyQueryService, CompanyQueryService>();
        services.AddScoped<Adrenalin.Modules.Company.Domain.Interfaces.ICompanyGroupRepository, CompanyGroupRepository>();

        // ── Routing / Group Management ────────────────────────────────────────
        services.AddScoped<IRoutingRuleRepository, RoutingRuleRepository>();
        services.AddScoped<IGroupAssignmentHistoryRepository, GroupAssignmentHistoryRepository>();

        // ── KnowledgeBase ─────────────────────────────────────────────────────
        services.AddScoped<Adrenalin.Modules.KB.Domain.Interfaces.IKbArticleRepository, KbArticleRepository>();
        services.AddScoped<Adrenalin.Modules.KB.Domain.Interfaces.IKbFolderRepository, KbFolderRepository>();
        services.AddScoped<Adrenalin.Modules.KB.Domain.Interfaces.IPortalBannerRepository, PortalBannerRepository>();

        // ── Notification ──────────────────────────────────────────────────────
        services.AddScoped<Adrenalin.Modules.Notification.Domain.Interfaces.INotificationRepository, NotificationRepository>();


        // ── Workflow ──────────────────────────────────────────────────────────
        services.AddScoped<IWorkflowValidator, WorkflowValidator>();
        services.AddScoped<Adrenalin.Modules.Workflow.Domain.Interfaces.IRoleNameLookup,
            Adrenalin.Persistence.Repositories.Workflow.RoleNameLookup>();

        return services;
    }
}