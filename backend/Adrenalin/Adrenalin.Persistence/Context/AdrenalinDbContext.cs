using Adrenalin.Modules.KB.Domain.Entities;
using Adrenalin.Modules.Ticketing.Domain.Enums;
using Adrenalin.Persistence.Configurations.Gamification;
using Adrenalin.SharedKernel.Interfaces;
using Adrenalin.SharedKernel.Mediator;
using Microsoft.EntityFrameworkCore;

namespace Adrenalin.Persistence.Context;

/// <summary>
/// Single application DbContext. Owns all entities across every module.
/// Implements IUnitOfWork — consumed by UnitOfWorkBehavior in the mediator pipeline.
/// After every successful save, dispatches domain events from any aggregate roots
/// (currently KbArticle and KbFolder) via IPublisher.
/// </summary>
public class AdrenalinDbContext : DbContext, IUnitOfWork
{
    private readonly IPublisher _publisher;

    public AdrenalinDbContext(DbContextOptions<AdrenalinDbContext> options, IPublisher publisher)
        : base(options)
    {
        _publisher = publisher;
    }

    // ── Auth ──────────────────────────────────────────────────────────────────
    public virtual DbSet<AgentBadge> AgentBadges { get; set; }
    public virtual DbSet<AgentChallenge> AgentChallenges { get; set; }
    public virtual DbSet<AgentPoint> AgentPoints { get; set; }
    public virtual DbSet<AgentStreak> AgentStreaks { get; set; }
    public virtual DbSet<AiSuggestionLog> AiSuggestionLogs { get; set; }
    public virtual DbSet<AuditLog> AuditLogs { get; set; }
    public virtual DbSet<AutoResolutionLog> AutoResolutionLogs { get; set; }
    public virtual DbSet<AutomationExecutionLog> AutomationExecutionLogs { get; set; }
    public virtual DbSet<AutomationRule> AutomationRules { get; set; }
    public virtual DbSet<Badge> Badges { get; set; }
    public virtual DbSet<BusinessHour> BusinessHours { get; set; }
    public virtual DbSet<Challenge> Challenges { get; set; }
    public virtual DbSet<Company> Companies { get; set; }
    public virtual DbSet<CompanyContactsLimit> CompanyContactsLimits { get; set; }
    public virtual DbSet<CompanyDomain> CompanyDomains { get; set; }
    public virtual DbSet<Contact> Contacts { get; set; }
    public virtual DbSet<CsatSurvey> CsatSurveys { get; set; }
    public virtual DbSet<CustomerStatusMap> CustomerStatusMaps { get; set; }
    public virtual DbSet<CustomerTier> CustomerTiers { get; set; }
    public virtual DbSet<EscalationRule> EscalationRules { get; set; }
    public virtual DbSet<GeoRegion> GeoRegions { get; set; }
    public virtual DbSet<Group> Groups { get; set; }
    public virtual DbSet<Holiday> Holidays { get; set; }

    // ── KnowledgeBase ─────────────────────────────────────────────────────────
    public virtual DbSet<KbArticle> KbArticles { get; set; }
    public virtual DbSet<KbAttachment> KbAttachments { get; set; }
    public virtual DbSet<KbFolder> KbFolders { get; set; }
    public virtual DbSet<PortalBanner> PortalBanners { get; set; }

    // ── Other modules ─────────────────────────────────────────────────────────
    public virtual DbSet<LeaderboardSnapshot> LeaderboardSnapshots { get; set; }
    public virtual DbSet<Module> Modules { get; set; }
    public virtual DbSet<NotificationLog> NotificationLogs { get; set; }
    public virtual DbSet<NotificationTemplate> NotificationTemplates { get; set; }
    public virtual DbSet<Permission> Permissions { get; set; }
    public virtual DbSet<PointRule> PointRules { get; set; }
    public virtual DbSet<ProductVersion> ProductVersions { get; set; }
    public virtual DbSet<RefreshToken> RefreshTokens { get; set; }
    public virtual DbSet<Role> Roles { get; set; }
    public virtual DbSet<RolePermission> RolePermissions { get; set; }
    public virtual DbSet<SlaPolicy> SlaPolicies { get; set; }
    public virtual DbSet<SlaTicket> SlaTickets { get; set; }
    public virtual DbSet<SolutionType> SolutionTypes { get; set; }
    public virtual DbSet<StatusTransition> StatusTransitions { get; set; }
    public virtual DbSet<SubModule> SubModules { get; set; }
    public virtual DbSet<Ticket> Tickets { get; set; }
    public virtual DbSet<TicketAssignmentLog> TicketAssignmentLogs { get; set; }
    public virtual DbSet<TicketAttachment> TicketAttachments { get; set; }
    public virtual DbSet<TicketClassification> TicketClassifications { get; set; }
    public virtual DbSet<TicketComment> TicketComments { get; set; }
    public virtual DbSet<TicketCustomField> TicketCustomFields { get; set; }
    public virtual DbSet<TicketRelation> TicketRelations { get; set; }
    public virtual DbSet<TicketWatcher> TicketWatchers { get; set; }
    public virtual DbSet<TicketRiskScore> TicketRiskScores { get; set; }
    public virtual DbSet<TicketStatusGraph> TicketStatusGraphs { get; set; }
    public virtual DbSet<TicketStatusGraphScope> TicketStatusGraphScopes { get; set; }
    public virtual DbSet<TicketStatusHistory> TicketStatusHistories { get; set; }
    public virtual DbSet<TokenBlacklist> TokenBlacklists { get; set; }
    public virtual DbSet<User> Users { get; set; }
    public virtual DbSet<UserGroup> UserGroups { get; set; }
    public virtual DbSet<UserOtpCode> UserOtpCodes { get; set; }
    public virtual DbSet<UserRole> UserRoles { get; set; }
    public virtual DbSet<UserSession> UserSessions { get; set; }
    public virtual DbSet<UserVerificationToken> UserVerificationTokens { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .HasPostgresEnum("ai", "agent_rating", new[] { "relevant", "moderately_relevant", "not_relevant" })
            .HasPostgresEnum("ai", "suggestion_type", new[] { "troubleshooting_steps", "similar_tickets" })
            .HasPostgresEnum("auth", "revocation_reason", new[] { "logout", "suspicious_reuse", "admin_force", "role_change" })
            .HasPostgresEnum("company", "deployment_model", new[] { "cloud", "on_premise", "hybrid" })
            .HasPostgresEnum("gamification", "badge_tier", new[] { "bronze", "silver", "gold", "platinum" })
            .HasPostgresEnum("gamification", "period_type", new[] { "daily", "weekly", "monthly", "all_time" })
            .HasPostgresEnum("notification", "delivery_status", new[] { "sent", "failed", "bounced", "pending" })
            .HasPostgresEnum("notification", "notification_channel", new[] { "email", "in_app", "sms" })
            .HasPostgresEnum("sla", "trigger_event", new[] { "ticket_created", "ticket_updated", "time_based" })
            .HasPostgresEnum("ticket", "ticket_priority", new[] { "urgent", "high", "medium", "low" })
            .HasPostgresEnum("ticket", "ticket_source", new[] { "email", "portal", "phone" })
            .HasPostgresEnum<TicketStatus>("ticket", "ticket_status")
            .HasPostgresEnum("ticket", "ticket_type", new[] { "bug", "enhancement", "incident", "change_request", "query", "service_request", "clarification", "environment_issue" })
            .HasPostgresExtension("btree_gin")
            .HasPostgresExtension("pg_trgm")
            .HasPostgresExtension("pgcrypto")
            .HasPostgresExtension("unaccent");

        // All entity configurations are loaded automatically from the Configurations folder
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AdrenalinDbContext).Assembly);

        modelBuilder.HasSequence("ticket_number_seq", "ticket");
    }

    /// <summary>
    /// Saves changes then dispatches domain events from aggregate roots (KB entities).
    /// </summary>
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var result = await base.SaveChangesAsync(cancellationToken);
        await DispatchDomainEventsAsync(cancellationToken);
        return result;
    }

    private async Task DispatchDomainEventsAsync(CancellationToken ct)
    {
        var articleEvents = ChangeTracker.Entries<KbArticle>()
            .SelectMany(e => e.Entity.DomainEvents).ToList();
        var folderEvents = ChangeTracker.Entries<KbFolder>()
            .SelectMany(e => e.Entity.DomainEvents).ToList();

        ChangeTracker.Entries<KbArticle>().ToList().ForEach(e => e.Entity.ClearDomainEvents());
        ChangeTracker.Entries<KbFolder>().ToList().ForEach(e => e.Entity.ClearDomainEvents());

        foreach (var evt in articleEvents.Concat(folderEvents))
            await _publisher.Publish(evt, ct);
    }
}
