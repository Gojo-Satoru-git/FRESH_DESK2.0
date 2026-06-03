using System;
using System.Collections.Generic;
using Adrenalin.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Adrenalin.Persistence.Context;

public partial class AdrenalinDbContext : DbContext
{
    public AdrenalinDbContext()
    {
    }

    public AdrenalinDbContext(DbContextOptions<AdrenalinDbContext> options)
        : base(options)
    {
    }

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

    public virtual DbSet<KbArticle> KbArticles { get; set; }

    public virtual DbSet<KbAttachment> KbAttachments { get; set; }

    public virtual DbSet<KbFolder> KbFolders { get; set; }

    public virtual DbSet<LeaderboardSnapshot> LeaderboardSnapshots { get; set; }

    public virtual DbSet<Module> Modules { get; set; }

    public virtual DbSet<NotificationLog> NotificationLogs { get; set; }

    public virtual DbSet<NotificationTemplate> NotificationTemplates { get; set; }

    public virtual DbSet<Permission> Permissions { get; set; }

    public virtual DbSet<PointRule> PointRules { get; set; }

    public virtual DbSet<PortalBanner> PortalBanners { get; set; }

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
            .HasPostgresEnum("ticket", "ticket_status", new[] { "new", "open", "in_progress", "pending_customer", "pending_internal", "on_hold", "product_roadmap", "pending_approval", "compliance_review", "dual_agent_confirm", "resolved", "reopened", "closed" })
            .HasPostgresEnum("ticket", "ticket_type", new[] { "bug", "enhancement", "incident", "change_request", "query", "service_request", "clarification", "environment_issue" })
            .HasPostgresExtension("btree_gin")
            .HasPostgresExtension("pg_trgm")
            .HasPostgresExtension("pgcrypto")
            .HasPostgresExtension("unaccent");

        modelBuilder.Entity<AgentBadge>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("agent_badges_pkey");

            entity.ToTable("agent_badges", "gamification");

            entity.HasIndex(e => new { e.AgentId, e.EarnedAt }, "idx_agent_badges_agent").IsDescending(false, true);

            entity.HasIndex(e => e.Notified, "idx_agent_badges_unnotified").HasFilter("(notified = false)");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.AgentId).HasColumnName("agent_id");
            entity.Property(e => e.BadgeId).HasColumnName("badge_id");
            entity.Property(e => e.EarnedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("earned_at");
            entity.Property(e => e.Notified).HasColumnName("notified");
            entity.Property(e => e.TicketId).HasColumnName("ticket_id");

            entity.HasOne(d => d.Agent).WithMany()
                .HasForeignKey(d => d.AgentId)
                .HasConstraintName("agent_badges_agent_id_fkey");

            entity.HasOne(d => d.Badge).WithMany()
                .HasForeignKey(d => d.BadgeId)
                .HasConstraintName("agent_badges_badge_id_fkey");

            entity.HasOne(d => d.Ticket).WithMany()
                .HasForeignKey(d => d.TicketId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("agent_badges_ticket_id_fkey");
        });

        modelBuilder.Entity<AgentChallenge>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("agent_challenges_pkey");

            entity.ToTable("agent_challenges", "gamification");

            entity.HasIndex(e => new { e.AgentId, e.IsCompleted }, "idx_agent_challenges_agent");

            entity.HasIndex(e => new { e.ChallengeId, e.IsCompleted }, "idx_agent_challenges_challenge");

            entity.HasIndex(e => new { e.AgentId, e.ChallengeId }, "uq_agent_challenges").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.AgentId).HasColumnName("agent_id");
            entity.Property(e => e.ChallengeId).HasColumnName("challenge_id");
            entity.Property(e => e.CompletedAt).HasColumnName("completed_at");
            entity.Property(e => e.CurrentValue).HasColumnName("current_value");
            entity.Property(e => e.IsCompleted).HasColumnName("is_completed");
            entity.Property(e => e.PointsAwarded).HasColumnName("points_awarded");

            entity.HasOne(d => d.Agent).WithMany()
                .HasForeignKey(d => d.AgentId)
                .HasConstraintName("agent_challenges_agent_id_fkey");

            entity.HasOne(d => d.Challenge).WithMany()
                .HasForeignKey(d => d.ChallengeId)
                .HasConstraintName("agent_challenges_challenge_id_fkey");
        });

        modelBuilder.Entity<AgentPoint>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("agent_points_pkey");

            entity.ToTable("agent_points", "gamification", tb => tb.HasComment("Immutable append-only point ledger. Event-sourcing pattern. Total = SUM(points). Enables full audit, point reversals (insert negative row), time-travel queries. Never update or delete — insert only."));

            entity.HasIndex(e => new { e.AgentId, e.EarnedAt }, "idx_agent_points_agent").IsDescending(false, true);

            entity.HasIndex(e => new { e.AgentId, e.RuleId, e.EarnedDate }, "idx_agent_points_rule_daily");

            entity.HasIndex(e => e.TicketId, "idx_agent_points_ticket").HasFilter("(ticket_id IS NOT NULL)");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.AgentId).HasColumnName("agent_id");
            entity.Property(e => e.Description)
                .HasMaxLength(200)
                .HasColumnName("description");
            entity.Property(e => e.EarnedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("earned_at");
            entity.Property(e => e.EarnedDate)
                .HasDefaultValueSql("((now() AT TIME ZONE 'UTC'::text))::date")
                .HasColumnName("earned_date");
            entity.Property(e => e.Points).HasColumnName("points");
            entity.Property(e => e.RuleId).HasColumnName("rule_id");
            entity.Property(e => e.TicketId).HasColumnName("ticket_id");

            entity.HasOne(d => d.Agent).WithMany()
                .HasForeignKey(d => d.AgentId)
                .HasConstraintName("agent_points_agent_id_fkey");

            entity.HasOne(d => d.Rule).WithMany()
                .HasForeignKey(d => d.RuleId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("agent_points_rule_id_fkey");

            entity.HasOne(d => d.Ticket).WithMany()
                .HasForeignKey(d => d.TicketId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("agent_points_ticket_id_fkey");
        });

        modelBuilder.Entity<AgentStreak>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("agent_streaks_pkey");

            entity.ToTable("agent_streaks", "gamification", tb => tb.HasComment("One row per agent. current_streak_days = consecutive days with ≥1 SLA-met resolution. Nightly job resets if last_activity_date < CURRENT_DATE - 1 day."));

            entity.HasIndex(e => e.AgentId, "uq_agent_streaks_agent").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.AgentId).HasColumnName("agent_id");
            entity.Property(e => e.CurrentStreakDays).HasColumnName("current_streak_days");
            entity.Property(e => e.LastActivityDate).HasColumnName("last_activity_date");
            entity.Property(e => e.LongestStreakDays).HasColumnName("longest_streak_days");
            entity.Property(e => e.StreakStartDate).HasColumnName("streak_start_date");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Agent).WithOne()
                .HasForeignKey<AgentStreak>(d => d.AgentId)
                .HasConstraintName("agent_streaks_agent_id_fkey");
        });

        modelBuilder.Entity<AiSuggestionLog>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("ai_suggestion_logs_pkey");

            entity.ToTable("ai_suggestion_logs", "ai", tb => tb.HasComment("AI suggestions shown to agent on new ticket. Two rows per ticket: troubleshooting_steps and similar_tickets. agent_rating feeds the ML training pipeline. ai_rating also stored redundantly on tickets.ai_rating for quick dashboard reporting."));

            entity.HasIndex(e => new { e.AgentId, e.CreatedAt }, "idx_ai_logs_agent").IsDescending(false, true);

            entity.HasIndex(e => e.TicketId, "idx_ai_logs_ticket");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.AgentId).HasColumnName("agent_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.SuggestionContent).HasColumnName("suggestion_content");
            entity.Property(e => e.TicketId).HasColumnName("ticket_id");

            entity.HasOne(d => d.Agent).WithMany()
                .HasForeignKey(d => d.AgentId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("ai_suggestion_logs_agent_id_fkey");

            entity.HasOne(d => d.Ticket).WithMany()
                .HasForeignKey(d => d.TicketId)
                .HasConstraintName("ai_suggestion_logs_ticket_id_fkey");
        });

        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("audit_log_pkey");

            entity.ToTable("audit_log", "audit", tb => tb.HasComment("Cross-schema forensic audit log. Populated by application layer or generic audit triggers. old_values/new_values store JSONB diff for immutable change history."));

            entity.HasIndex(e => new { e.TableName, e.RecordId, e.ChangedAt }, "idx_audit_log_record").IsDescending(false, false, true);

            entity.HasIndex(e => new { e.TableName, e.ChangedAt }, "idx_audit_log_table").IsDescending(false, true);

            entity.HasIndex(e => new { e.ChangedBy, e.ChangedAt }, "idx_audit_log_user").IsDescending(false, true);

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.Action)
                .HasMaxLength(10)
                .HasColumnName("action");
            entity.Property(e => e.ChangedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("changed_at");
            entity.Property(e => e.ChangedBy).HasColumnName("changed_by");
            entity.Property(e => e.IpAddress).HasColumnName("ip_address");
            entity.Property(e => e.NewValues)
                .HasColumnType("jsonb")
                .HasColumnName("new_values");
            entity.Property(e => e.OldValues)
                .HasColumnType("jsonb")
                .HasColumnName("old_values");
            entity.Property(e => e.RecordId).HasColumnName("record_id");
            entity.Property(e => e.SchemaName)
                .HasMaxLength(60)
                .HasColumnName("schema_name");
            entity.Property(e => e.SessionId).HasColumnName("session_id");
            entity.Property(e => e.TableName)
                .HasMaxLength(60)
                .HasColumnName("table_name");

            entity.HasOne(d => d.ChangedByNavigation).WithMany()
                .HasForeignKey(d => d.ChangedBy)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("audit_log_changed_by_fkey");
        });

        modelBuilder.Entity<AutoResolutionLog>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("auto_resolution_log_pkey");

            entity.ToTable("auto_resolution_log", "ai", tb => tb.HasComment("Full audit of auto-resolution engine activity. One row per resolution attempt (successful or blocked). was_reopened=TRUE is the primary negative signal for the learning loop — the learning job reads this to raise confidence_threshold on kb_articles with high reopen rates. blocked_guardrail action means payroll/financial/legal guardrail fired."));

            entity.HasIndex(e => new { e.ActionTaken, e.MatchedAt }, "idx_auto_res_action").IsDescending(false, true);

            entity.HasIndex(e => new { e.KbArticleId, e.MatchedAt }, "idx_auto_res_article").IsDescending(false, true);

            entity.HasIndex(e => new { e.WasReopened, e.MatchedAt }, "idx_auto_res_reopened")
                .IsDescending(false, true)
                .HasFilter("(was_reopened = true)");

            entity.HasIndex(e => new { e.TicketId, e.MatchedAt }, "idx_auto_res_ticket").IsDescending(false, true);

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.ActionTaken)
                .HasMaxLength(30)
                .HasColumnName("action_taken");
            entity.Property(e => e.BlockedReason).HasColumnName("blocked_reason");
            entity.Property(e => e.ConfidenceThreshold)
                .HasPrecision(5, 4)
                .HasColumnName("confidence_threshold");
            entity.Property(e => e.FinalConfidence)
                .HasPrecision(5, 4)
                .HasColumnName("final_confidence");
            entity.Property(e => e.KbArticleId).HasColumnName("kb_article_id");
            entity.Property(e => e.KeywordMatches).HasColumnName("keyword_matches");
            entity.Property(e => e.MatchPhase)
                .HasMaxLength(20)
                .HasColumnName("match_phase");
            entity.Property(e => e.MatchedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("matched_at");
            entity.Property(e => e.ReopenReason).HasColumnName("reopen_reason");
            entity.Property(e => e.ReopenedAt).HasColumnName("reopened_at");
            entity.Property(e => e.ResolutionChannel)
                .HasMaxLength(20)
                .HasColumnName("resolution_channel");
            entity.Property(e => e.ResolutionSentAt).HasColumnName("resolution_sent_at");
            entity.Property(e => e.SemanticSimilarity)
                .HasPrecision(5, 4)
                .HasColumnName("semantic_similarity");
            entity.Property(e => e.TicketId).HasColumnName("ticket_id");
            entity.Property(e => e.WasReopened).HasColumnName("was_reopened");

            entity.HasOne(d => d.KbArticle).WithMany(p => p.AutoResolutionLogs)
                .HasForeignKey(d => d.KbArticleId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("auto_resolution_log_kb_article_id_fkey");

            entity.HasOne(d => d.Ticket).WithMany(p => p.AutoResolutionLogs)
                .HasForeignKey(d => d.TicketId)
                .HasConstraintName("auto_resolution_log_ticket_id_fkey");
        });

        modelBuilder.Entity<AutomationExecutionLog>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("automation_execution_log_pkey");

            entity.ToTable("automation_execution_log", "sla");

            entity.HasIndex(e => new { e.Succeeded, e.ExecutedAt }, "idx_ael_failed").HasFilter("(succeeded = false)");

            entity.HasIndex(e => new { e.RuleId, e.ExecutedAt }, "idx_ael_rule").IsDescending(false, true);

            entity.HasIndex(e => new { e.TicketId, e.ExecutedAt }, "idx_ael_ticket").IsDescending(false, true);

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.ErrorMessage).HasColumnName("error_message");
            entity.Property(e => e.ExecutedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("executed_at");
            entity.Property(e => e.RuleId).HasColumnName("rule_id");
            entity.Property(e => e.Succeeded)
                .HasDefaultValue(true)
                .HasColumnName("succeeded");
            entity.Property(e => e.TicketId).HasColumnName("ticket_id");

            entity.HasOne(d => d.Rule).WithMany(p => p.AutomationExecutionLogs)
                .HasForeignKey(d => d.RuleId)
                .HasConstraintName("automation_execution_log_rule_id_fkey");

            entity.HasOne(d => d.Ticket).WithMany(p => p.AutomationExecutionLogs)
                .HasForeignKey(d => d.TicketId)
                .HasConstraintName("automation_execution_log_ticket_id_fkey");
        });

        modelBuilder.Entity<AutomationRule>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("automation_rules_pkey");

            entity.ToTable("automation_rules", "sla", tb => tb.HasComment("JSONB conditions/actions allows rule engine to evolve without schema migrations. Examples: route to group on creation, reopen on reply, auto-close at day 15, escalate on no-response, set payment hold status. execution_order determines eval sequence."));

            entity.HasIndex(e => e.Actions, "idx_automation_rules_actions").HasMethod("gin");

            entity.HasIndex(e => e.Conditions, "idx_automation_rules_conditions").HasMethod("gin");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.Actions)
                .HasDefaultValueSql("'[]'::jsonb")
                .HasColumnType("jsonb")
                .HasColumnName("actions");
            entity.Property(e => e.Conditions)
                .HasDefaultValueSql("'[]'::jsonb")
                .HasColumnType("jsonb")
                .HasColumnName("conditions");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.ExecutionOrder)
                .HasDefaultValue(100)
                .HasColumnName("execution_order");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.IsDeleted).HasColumnName("is_deleted");
            entity.Property(e => e.Name)
                .HasMaxLength(150)
                .HasColumnName("name");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("updated_at");
            entity.Property(e => e.UpdatedBy).HasColumnName("updated_by");

            entity.HasOne(d => d.CreatedByNavigation).WithMany()
                .HasForeignKey(d => d.CreatedBy)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("automation_rules_created_by_fkey");

            entity.HasOne(d => d.UpdatedByNavigation).WithMany()
                .HasForeignKey(d => d.UpdatedBy)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("automation_rules_updated_by_fkey");
        });

        modelBuilder.Entity<Badge>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("badges_pkey");

            entity.ToTable("badges", "gamification");

            entity.HasIndex(e => e.Code, "uq_badges_code").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.Code)
                .HasMaxLength(80)
                .HasColumnName("code");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.IconUrl).HasColumnName("icon_url");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.IsDeleted).HasColumnName("is_deleted");
            entity.Property(e => e.Name)
                .HasMaxLength(150)
                .HasColumnName("name");
            entity.Property(e => e.PointsValue).HasColumnName("points_value");

            entity.HasOne(d => d.CreatedByNavigation).WithMany()
                .HasForeignKey(d => d.CreatedBy)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("badges_created_by_fkey");
        });

        modelBuilder.Entity<BusinessHour>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("business_hours_pkey");

            entity.ToTable("business_hours", "sla");

            entity.HasIndex(e => e.GeoRegion, "idx_business_hours_region").HasFilter("(is_deleted = false)");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.EndTime).HasColumnName("end_time");
            entity.Property(e => e.GeoRegion)
                .HasMaxLength(20)
                .HasColumnName("geo_region");
            entity.Property(e => e.IsDeleted).HasColumnName("is_deleted");
            entity.Property(e => e.StartTime).HasColumnName("start_time");
            entity.Property(e => e.WorkingDays)
                .HasMaxLength(20)
                .HasDefaultValueSql("'Mon-Fri'::character varying")
                .HasColumnName("working_days");

            entity.HasOne(d => d.CreatedByNavigation).WithMany()
                .HasForeignKey(d => d.CreatedBy)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("business_hours_created_by_fkey");

            entity.HasOne(d => d.GeoRegionNavigation).WithMany()
                .HasForeignKey(d => d.GeoRegion)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("business_hours_geo_region_fkey");
        });

        modelBuilder.Entity<Challenge>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("challenges_pkey");

            entity.ToTable("challenges", "gamification");

            entity.HasIndex(e => new { e.IsActive, e.StartsAt, e.EndsAt }, "idx_challenges_active");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.BadgeId).HasColumnName("badge_id");
            entity.Property(e => e.BonusPoints).HasColumnName("bonus_points");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.EndsAt).HasColumnName("ends_at");
            entity.Property(e => e.GroupId).HasColumnName("group_id");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.Metric)
                .HasMaxLength(80)
                .HasColumnName("metric");
            entity.Property(e => e.Scope)
                .HasMaxLength(20)
                .HasDefaultValueSql("'global'::character varying")
                .HasColumnName("scope");
            entity.Property(e => e.StartsAt).HasColumnName("starts_at");
            entity.Property(e => e.TargetValue).HasColumnName("target_value");
            entity.Property(e => e.Title)
                .HasMaxLength(200)
                .HasColumnName("title");

            entity.HasOne(d => d.Badge).WithMany()
                .HasForeignKey(d => d.BadgeId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("challenges_badge_id_fkey");

            entity.HasOne(d => d.CreatedByNavigation).WithMany()
                .HasForeignKey(d => d.CreatedBy)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("challenges_created_by_fkey");

            entity.HasOne(d => d.Group).WithMany()
                .HasForeignKey(d => d.GroupId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("challenges_group_id_fkey");
        });

        modelBuilder.Entity<Company>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("companies_pkey");

            entity.ToTable("companies", "company", tb => tb.HasComment("Core customer account. health_score + customer_tiers.priority_bump elevates ticket priority. delivery_support_active routes tickets to delivery (hypercare). payment_on_hold triggers auto On Hold status for new tickets."));

            entity.HasIndex(e => e.GeoRegion, "idx_companies_geo_region").HasFilter("(is_deleted = false)");

            entity.HasIndex(e => e.HealthScore, "idx_companies_health_score")
                .IsDescending()
                .HasFilter("(is_deleted = false)");

            entity.HasIndex(e => e.Name, "idx_companies_name_trgm")
                .HasFilter("(is_deleted = false)")
                .HasMethod("gin")
                .HasOperators(new[] { "gin_trgm_ops" });

            entity.HasIndex(e => e.PaymentOnHold, "idx_companies_payment_hold").HasFilter("((payment_on_hold = true) AND (is_deleted = false))");

            entity.HasIndex(e => e.SupportTier, "idx_companies_tier").HasFilter("(is_deleted = false)");

            entity.HasIndex(e => e.CspId, "uq_companies_csp_id").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.CamUserId).HasColumnName("cam_user_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.CspId)
                .HasMaxLength(100)
                .HasColumnName("csp_id");
            entity.Property(e => e.DeliveryManagerId).HasColumnName("delivery_manager_id");
            entity.Property(e => e.DeliverySupportActive).HasColumnName("delivery_support_active");
            entity.Property(e => e.GeoRegion)
                .HasMaxLength(20)
                .HasColumnName("geo_region");
            entity.Property(e => e.HealthScore)
                .HasDefaultValue(75)
                .HasColumnName("health_score");
            entity.Property(e => e.Industry)
                .HasMaxLength(100)
                .HasColumnName("industry");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.IsDeleted).HasColumnName("is_deleted");
            entity.Property(e => e.IsPayrollCustomer).HasColumnName("is_payroll_customer");
            entity.Property(e => e.LeaveCreditCycle)
                .HasMaxLength(30)
                .HasColumnName("leave_credit_cycle");
            entity.Property(e => e.MigrationDate).HasColumnName("migration_date");
            entity.Property(e => e.ModifiedAt).HasColumnName("modified_at");
            entity.Property(e => e.ModifiedBy)
                .HasMaxLength(100)
                .HasColumnName("modified_by");
            entity.Property(e => e.Name)
                .HasMaxLength(200)
                .HasColumnName("name");
            entity.Property(e => e.Notes).HasColumnName("notes");
            entity.Property(e => e.PaymentOnHold).HasColumnName("payment_on_hold");
            entity.Property(e => e.SupportTier)
                .HasMaxLength(10)
                .HasColumnName("support_tier");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("updated_at");
            entity.Property(e => e.UpdatedBy).HasColumnName("updated_by");

            entity.HasOne(d => d.CamUser).WithMany()
                .HasForeignKey(d => d.CamUserId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("companies_cam_user_id_fkey");

            entity.HasOne(d => d.CreatedByNavigation).WithMany()
                .HasForeignKey(d => d.CreatedBy)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("companies_created_by_fkey");

            entity.HasOne(d => d.DeliveryManager).WithMany()
                .HasForeignKey(d => d.DeliveryManagerId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("companies_delivery_manager_id_fkey");

            entity.HasOne(d => d.GeoRegionNavigation).WithMany()
                .HasForeignKey(d => d.GeoRegion)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("companies_geo_region_fkey");

            entity.HasOne(d => d.SupportTierNavigation).WithMany()
                .HasForeignKey(d => d.SupportTier)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("companies_support_tier_fkey");

            entity.HasOne(d => d.UpdatedByNavigation).WithMany()
                .HasForeignKey(d => d.UpdatedBy)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("companies_updated_by_fkey");
        });

        modelBuilder.Entity<CompanyContactsLimit>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("company_contacts_limit_pkey");

            entity.ToTable("company_contacts_limit", "company");

            entity.HasIndex(e => e.CompanyId, "uq_company_contacts_limit_company").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.CompanyId).HasColumnName("company_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.MaxContacts)
                .HasDefaultValue(10)
                .HasColumnName("max_contacts");

            entity.HasOne(d => d.Company).WithOne(p => p.CompanyContactsLimit)
                .HasForeignKey<CompanyContactsLimit>(d => d.CompanyId)
                .HasConstraintName("company_contacts_limit_company_id_fkey");

            entity.HasOne(d => d.CreatedByNavigation).WithMany()
                .HasForeignKey(d => d.CreatedBy)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("company_contacts_limit_created_by_fkey");
        });

        modelBuilder.Entity<CompanyDomain>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("company_domains_pkey");

            entity.ToTable("company_domains", "company", tb => tb.HasComment("Email domains owned by each company. UNIQUE on lower(domain) enables O(1) auto-routing of incoming emails to the correct company account."));

            entity.HasIndex(e => e.CompanyId, "idx_company_domains_company").HasFilter("(is_deleted = false)");

            entity.HasIndex(e => e.Domain, "idx_company_domains_domain");

            entity.HasIndex(e => e.Domain, "uq_company_domains_domain").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.CompanyId).HasColumnName("company_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.CreatedBy)
                .HasMaxLength(100)
                .HasDefaultValueSql("'system'::character varying")
                .HasColumnName("created_by");
            entity.Property(e => e.Domain)
                .HasMaxLength(255)
                .HasColumnName("domain");
            entity.Property(e => e.IsDeleted).HasColumnName("is_deleted");
            entity.Property(e => e.IsPrimary).HasColumnName("is_primary");

            entity.HasOne(d => d.Company).WithMany(p => p.CompanyDomains)
                .HasForeignKey(d => d.CompanyId)
                .HasConstraintName("company_domains_company_id_fkey");
        });

        modelBuilder.Entity<Contact>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("contacts_pkey");

            entity.ToTable("contacts", "company", tb => tb.HasComment("Authorized contacts per company (5-20 per account). is_authorized=false blocks ticket creation. auto_created=true when system creates contact from unknown inbound email domain match."));

            entity.HasIndex(e => new { e.CompanyId, e.IsAuthorized }, "idx_contacts_authorized").HasFilter("(is_deleted = false)");

            entity.HasIndex(e => e.CompanyId, "idx_contacts_company").HasFilter("(is_deleted = false)");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.AutoCreated).HasColumnName("auto_created");
            entity.Property(e => e.CompanyId).HasColumnName("company_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.Email)
                .HasMaxLength(255)
                .HasColumnName("email");
            entity.Property(e => e.IsAuthorized)
                .HasDefaultValue(true)
                .HasColumnName("is_authorized");
            entity.Property(e => e.IsDeleted).HasColumnName("is_deleted");
            entity.Property(e => e.ModifiedAt).HasColumnName("modified_at");
            entity.Property(e => e.ModifiedBy)
                .HasMaxLength(100)
                .HasColumnName("modified_by");
            entity.Property(e => e.Name)
                .HasMaxLength(200)
                .HasColumnName("name");
            entity.Property(e => e.Phone)
                .HasMaxLength(30)
                .HasColumnName("phone");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("updated_at");
            entity.Property(e => e.UpdatedBy).HasColumnName("updated_by");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.Company).WithMany(p => p.Contacts)
                .HasForeignKey(d => d.CompanyId)
                .HasConstraintName("contacts_company_id_fkey");

            entity.HasOne(d => d.CreatedByNavigation).WithMany()
                .HasForeignKey(d => d.CreatedBy)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("contacts_created_by_fkey");

            entity.HasOne(d => d.UpdatedByNavigation).WithMany()
                .HasForeignKey(d => d.UpdatedBy)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("contacts_updated_by_fkey");

            entity.HasOne(d => d.User).WithMany()
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("contacts_user_id_fkey");
        });

        modelBuilder.Entity<CsatSurvey>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("csat_surveys_pkey");

            entity.ToTable("csat_surveys", "ticket");

            entity.HasIndex(e => e.ContactId, "idx_csat_surveys_contact");

            entity.HasIndex(e => e.Rating, "idx_csat_surveys_rating");

            entity.HasIndex(e => e.TicketId, "uq_csat_surveys_ticket").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.ContactId).HasColumnName("contact_id");
            entity.Property(e => e.Feedback).HasColumnName("feedback");
            entity.Property(e => e.Rating).HasColumnName("rating");
            entity.Property(e => e.SubmittedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("submitted_at");
            entity.Property(e => e.TicketId).HasColumnName("ticket_id");

            entity.HasOne(d => d.Contact).WithMany(p => p.CsatSurveys)
                .HasForeignKey(d => d.ContactId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("csat_surveys_contact_id_fkey");

            entity.HasOne(d => d.Ticket).WithOne(p => p.CsatSurvey)
                .HasForeignKey<CsatSurvey>(d => d.TicketId)
                .HasConstraintName("csat_surveys_ticket_id_fkey");
        });

        modelBuilder.Entity<CustomerStatusMap>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("customer_status_map_pkey");

            entity.ToTable("customer_status_map", "config", tb => tb.HasComment("Maps every agent-facing ticket status to the simplified customer-facing label. Customer portal API reads this table — customers never see statuses like pending_internal or compliance_review. Only Open, Reopen, Closed are exposed."));

            entity.HasIndex(e => e.AgentStatus, "uq_customer_status_map").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.AgentStatus)
                .HasMaxLength(40)
                .HasColumnName("agent_status");
            entity.Property(e => e.CustomerDescription)
                .HasMaxLength(200)
                .HasColumnName("customer_description");
            entity.Property(e => e.CustomerLabel)
                .HasMaxLength(40)
                .HasColumnName("customer_label");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
        });

        // CustomerTier configuration is loaded from Configurations


        modelBuilder.Entity<EscalationRule>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("escalation_rules_pkey");

            entity.ToTable("escalation_rules", "sla");

            entity.HasIndex(e => new { e.IsActive, e.NoResponseMinutes }, "idx_escalation_rules_active");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
            entity.Property(e => e.NoResponseMinutes).HasColumnName("no_response_minutes");
            entity.Property(e => e.NotifyRole)
                .HasMaxLength(60)
                .HasColumnName("notify_role");

            entity.HasOne(d => d.CreatedByNavigation).WithMany()
                .HasForeignKey(d => d.CreatedBy)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("escalation_rules_created_by_fkey");
        });

        // GeoRegion and Group configurations are loaded from Configurations


        modelBuilder.Entity<Holiday>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("holidays_pkey");

            entity.ToTable("holidays", "sla");

            entity.HasIndex(e => new { e.GeoRegion, e.HolidayDate }, "idx_holidays_region_date").HasFilter("(is_deleted = false)");

            entity.HasIndex(e => new { e.GeoRegion, e.HolidayDate }, "uq_holidays").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.GeoRegion)
                .HasMaxLength(20)
                .HasColumnName("geo_region");
            entity.Property(e => e.HolidayDate).HasColumnName("holiday_date");
            entity.Property(e => e.IsDeleted).HasColumnName("is_deleted");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");

            entity.HasOne(d => d.CreatedByNavigation).WithMany()
                .HasForeignKey(d => d.CreatedBy)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("holidays_created_by_fkey");

            entity.HasOne(d => d.GeoRegionNavigation).WithMany()
                .HasForeignKey(d => d.GeoRegion)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("holidays_geo_region_fkey");
        });

        modelBuilder.Entity<KbArticle>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("kb_articles_pkey");

            entity.ToTable("kb_articles", "kb");

            entity.HasIndex(e => new { e.AutoResolve, e.GuardrailExcluded }, "idx_kb_articles_auto_resolve").HasFilter("((auto_resolve = true) AND (is_deleted = false))");

            entity.HasIndex(e => e.FolderId, "idx_kb_articles_folder").HasFilter("(is_deleted = false)");

            entity.HasIndex(e => e.Keywords, "idx_kb_articles_keywords")
                .HasFilter("((auto_resolve = true) AND (is_deleted = false))")
                .HasMethod("gin");

            entity.HasIndex(e => new { e.IsPublished, e.ArticleType }, "idx_kb_articles_published").HasFilter("(is_deleted = false)");

            entity.HasIndex(e => e.Title, "idx_kb_articles_title_trgm")
                .HasFilter("(is_deleted = false)")
                .HasMethod("gin")
                .HasOperators(new[] { "gin_trgm_ops" });

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.ArticleType)
                .HasMaxLength(40)
                .HasColumnName("article_type");
            entity.Property(e => e.AuthorId).HasColumnName("author_id");
            entity.Property(e => e.AutoResolve)
                .HasComment("TRUE = this article is eligible for the auto-resolution engine. Engine only fires if confidence > confidence_threshold AND guardrail_excluded = FALSE.")
                .HasColumnName("auto_resolve");
            entity.Property(e => e.ConfidenceThreshold)
                .HasPrecision(4, 3)
                .HasDefaultValue(0.850m)
                .HasComment("Minimum match confidence (0.85 default) required to trigger auto-resolve. Articles with high reopen rates should have this raised automatically by learning loop.")
                .HasColumnName("confidence_threshold");
            entity.Property(e => e.Content).HasColumnName("content");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.FolderId).HasColumnName("folder_id");
            entity.Property(e => e.GuardrailExcluded)
                .HasComment("TRUE = this article covers a guardrail topic (payroll, financial, legal/compliance). Auto-resolution engine NEVER fires for guardrail_excluded articles regardless of confidence.")
                .HasColumnName("guardrail_excluded");
            entity.Property(e => e.IsDeleted).HasColumnName("is_deleted");
            entity.Property(e => e.IsPublished).HasColumnName("is_published");
            entity.Property(e => e.Keywords)
                .HasComment("Phase 1 (keyword match) trigger words. Stored as PostgreSQL text array. Example: ARRAY['forgot password', 'reset password', 'login failed'].")
                .HasColumnName("keywords");
            entity.Property(e => e.ResolutionText).HasColumnName("resolution_text");
            entity.Property(e => e.Status)
                .HasMaxLength(30)
                .HasDefaultValueSql("'draft'::character varying")
                .HasColumnName("status");
            entity.Property(e => e.TimesMatched)
                .HasComment("Learning loop counter: incremented each time this article is matched (auto-resolve attempted).")
                .HasColumnName("times_matched");
            entity.Property(e => e.TimesReopened)
                .HasComment("Learning loop counter: incremented each time a ticket auto-resolved via this article is reopened. High reopen rate → confidence_threshold auto-raised by learning loop job.")
                .HasColumnName("times_reopened");
            entity.Property(e => e.Title)
                .HasMaxLength(300)
                .HasColumnName("title");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("updated_at");
            entity.Property(e => e.UpdatedBy).HasColumnName("updated_by");

            entity.HasOne(d => d.Author).WithMany()
                .HasForeignKey(d => d.AuthorId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("kb_articles_author_id_fkey");

            entity.HasOne(d => d.CreatedByNavigation).WithMany()
                .HasForeignKey(d => d.CreatedBy)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("kb_articles_created_by_fkey");

            entity.HasOne(d => d.Folder).WithMany(p => p.KbArticles)
                .HasForeignKey(d => d.FolderId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("kb_articles_folder_id_fkey");

            entity.HasOne(d => d.UpdatedByNavigation).WithMany()
                .HasForeignKey(d => d.UpdatedBy)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("kb_articles_updated_by_fkey");
        });

        modelBuilder.Entity<KbAttachment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("kb_attachments_pkey");

            entity.ToTable("kb_attachments", "kb");

            entity.HasIndex(e => e.ArticleId, "idx_kb_attachments_article").HasFilter("(is_deleted = false)");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.ArticleId).HasColumnName("article_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.FileName)
                .HasMaxLength(255)
                .HasColumnName("file_name");
            entity.Property(e => e.FileSizeBytes).HasColumnName("file_size_bytes");
            entity.Property(e => e.FileUrl).HasColumnName("file_url");
            entity.Property(e => e.IsDeleted).HasColumnName("is_deleted");
            entity.Property(e => e.MimeType)
                .HasMaxLength(100)
                .HasColumnName("mime_type");

            entity.HasOne(d => d.Article).WithMany(p => p.KbAttachments)
                .HasForeignKey(d => d.ArticleId)
                .HasConstraintName("kb_attachments_article_id_fkey");
        });

        modelBuilder.Entity<KbFolder>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("kb_folders_pkey");

            entity.ToTable("kb_folders", "kb", tb => tb.HasComment("Self-referencing folder hierarchy. parent_id=NULL for root folders. Use WITH RECURSIVE CTE to retrieve full tree. Depth limit enforced in API layer."));

            entity.HasIndex(e => e.ParentId, "idx_kb_folders_parent").HasFilter("(is_deleted = false)");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.DisplayOrder).HasColumnName("display_order");
            entity.Property(e => e.IsDeleted).HasColumnName("is_deleted");
            entity.Property(e => e.Name)
                .HasMaxLength(150)
                .HasColumnName("name");
            entity.Property(e => e.ParentId).HasColumnName("parent_id");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("updated_at");
            entity.Property(e => e.UpdatedBy).HasColumnName("updated_by");

            entity.HasOne(d => d.CreatedByNavigation).WithMany()
                .HasForeignKey(d => d.CreatedBy)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("kb_folders_created_by_fkey");

            entity.HasOne(d => d.Parent).WithMany(p => p.InverseParent)
                .HasForeignKey(d => d.ParentId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("kb_folders_parent_id_fkey");

            entity.HasOne(d => d.UpdatedByNavigation).WithMany()
                .HasForeignKey(d => d.UpdatedBy)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("kb_folders_updated_by_fkey");
        });

        modelBuilder.Entity<LeaderboardSnapshot>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("leaderboard_snapshots_pkey");

            entity.ToTable("leaderboard_snapshots", "gamification", tb => tb.HasComment("Pre-computed daily/weekly/monthly leaderboards. Rebuilt at 00:05 by scheduled job. Reading live SUM on agent_points at request time is too expensive at scale."));

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.AgentId).HasColumnName("agent_id");
            entity.Property(e => e.AvgCsat)
                .HasPrecision(4, 2)
                .HasColumnName("avg_csat");
            entity.Property(e => e.ComputedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("computed_at");
            entity.Property(e => e.GroupId).HasColumnName("group_id");
            entity.Property(e => e.PeriodEnd).HasColumnName("period_end");
            entity.Property(e => e.PeriodStart).HasColumnName("period_start");
            entity.Property(e => e.Rank).HasColumnName("rank");
            entity.Property(e => e.SlaMetCount).HasColumnName("sla_met_count");
            entity.Property(e => e.TicketsResolved).HasColumnName("tickets_resolved");
            entity.Property(e => e.TotalPoints).HasColumnName("total_points");

            entity.HasOne(d => d.Agent).WithMany()
                .HasForeignKey(d => d.AgentId)
                .HasConstraintName("leaderboard_snapshots_agent_id_fkey");

            entity.HasOne(d => d.Group).WithMany()
                .HasForeignKey(d => d.GroupId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("leaderboard_snapshots_group_id_fkey");
        });

        // Module configuration is loaded from Configurations

        modelBuilder.Entity<NotificationLog>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("notification_logs_pkey");

            entity.ToTable("notification_logs", "notification");

            entity.HasIndex(e => new { e.IsFailedDelivery, e.SentAt }, "idx_notification_logs_failed").HasFilter("(is_failed_delivery = true)");

            entity.HasIndex(e => e.TemplateId, "idx_notification_logs_template");

            entity.HasIndex(e => new { e.TicketId, e.SentAt }, "idx_notification_logs_ticket")
                .IsDescending(false, true)
                .HasFilter("(ticket_id IS NOT NULL)");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.ErrorMessage).HasColumnName("error_message");
            entity.Property(e => e.IsFailedDelivery).HasColumnName("is_failed_delivery");
            entity.Property(e => e.RecipientEmail)
                .HasMaxLength(255)
                .HasColumnName("recipient_email");
            entity.Property(e => e.SentAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("sent_at");
            entity.Property(e => e.TemplateId).HasColumnName("template_id");
            entity.Property(e => e.TicketId).HasColumnName("ticket_id");

            entity.HasOne(d => d.Template).WithMany(p => p.NotificationLogs)
                .HasForeignKey(d => d.TemplateId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("notification_logs_template_id_fkey");

            entity.HasOne(d => d.Ticket).WithMany(p => p.NotificationLogs)
                .HasForeignKey(d => d.TicketId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("notification_logs_ticket_id_fkey");
        });

        modelBuilder.Entity<NotificationTemplate>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("notification_templates_pkey");

            entity.ToTable("notification_templates", "notification", tb => tb.HasComment("Handlebars templates for all notification events. code is the stable reference key. Examples: TICKET_CREATED, AGENT_REPLY, CSAT_SURVEY, SLA_BREACH, BADGE_AWARDED."));

            entity.HasIndex(e => e.Code, "uq_notification_templates_code").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.BodyHtml).HasColumnName("body_html");
            entity.Property(e => e.Code)
                .HasMaxLength(80)
                .HasColumnName("code");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.IsDeleted).HasColumnName("is_deleted");
            entity.Property(e => e.Name)
                .HasMaxLength(150)
                .HasColumnName("name");
            entity.Property(e => e.Subject)
                .HasMaxLength(300)
                .HasColumnName("subject");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("updated_at");
            entity.Property(e => e.UpdatedBy).HasColumnName("updated_by");

            entity.HasOne(d => d.CreatedByNavigation).WithMany()
                .HasForeignKey(d => d.CreatedBy)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("notification_templates_created_by_fkey");

            entity.HasOne(d => d.UpdatedByNavigation).WithMany()
                .HasForeignKey(d => d.UpdatedBy)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("notification_templates_updated_by_fkey");
        });

        // Permission configuration is loaded from Configurations


        modelBuilder.Entity<PointRule>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("point_rules_pkey");

            entity.ToTable("point_rules", "gamification", tb => tb.HasComment("Defines what actions earn points. max_per_day prevents farming: engine checks SUM(points) WHERE agent_id=X AND rule_id=Y AND earned_at::date = TODAY. Negative points allowed for reversals."));

            entity.HasIndex(e => e.Code, "uq_point_rules_code").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.Code)
                .HasMaxLength(80)
                .HasColumnName("code");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.MaxPerDay).HasColumnName("max_per_day");
            entity.Property(e => e.Points).HasColumnName("points");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.CreatedByNavigation).WithMany()
                .HasForeignKey(d => d.CreatedBy)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("point_rules_created_by_fkey");
        });

        modelBuilder.Entity<PortalBanner>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("portal_banners_pkey");

            entity.ToTable("portal_banners", "kb");

            entity.HasIndex(e => new { e.IsActive, e.ActiveFrom, e.ActiveTo }, "idx_portal_banners_active").HasFilter("(is_active = true)");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.ActiveFrom).HasColumnName("active_from");
            entity.Property(e => e.ActiveTo).HasColumnName("active_to");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.Message).HasColumnName("message");
            entity.Property(e => e.Title)
                .HasMaxLength(200)
                .HasColumnName("title");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("updated_at");
            entity.Property(e => e.UpdatedBy).HasColumnName("updated_by");

            entity.HasOne(d => d.CreatedByNavigation).WithMany()
                .HasForeignKey(d => d.CreatedBy)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("portal_banners_created_by_fkey");

            entity.HasOne(d => d.UpdatedByNavigation).WithMany()
                .HasForeignKey(d => d.UpdatedBy)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("portal_banners_updated_by_fkey");
        });

        // ProductVersion, RefreshToken, Role, and RolePermission configurations are loaded from Configurations



        modelBuilder.Entity<SlaPolicy>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("sla_policies_pkey");

            entity.ToTable("sla_policies", "sla", tb => tb.HasComment("One row per (geo_region × tier × priority). First response: Urgent=120m, High=240m, Med=360m, Low=480m. Resolution: Low=2880m (48 biz hrs). SLA minutes are business-hours only, not wall-clock."));

            entity.HasIndex(e => new { e.GeoRegion, e.TierCode, e.Priority }, "idx_sla_policies_lookup").HasFilter("(is_active = true)");

            entity.HasIndex(e => new { e.GeoRegion, e.TierCode, e.Priority }, "uq_sla_policies").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.FirstResponseMinutes).HasColumnName("first_response_minutes");
            entity.Property(e => e.GeoRegion)
                .HasMaxLength(20)
                .HasColumnName("geo_region");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.IsDeleted).HasColumnName("is_deleted");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
            entity.Property(e => e.Priority)
                .HasMaxLength(20)
                .HasColumnName("priority");
            entity.Property(e => e.ResolutionMinutes).HasColumnName("resolution_minutes");
            entity.Property(e => e.TierCode)
                .HasMaxLength(10)
                .HasColumnName("tier_code");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("updated_at");
            entity.Property(e => e.UpdatedBy).HasColumnName("updated_by");

            entity.HasOne(d => d.CreatedByNavigation).WithMany()
                .HasForeignKey(d => d.CreatedBy)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("sla_policies_created_by_fkey");

            entity.HasOne(d => d.GeoRegionNavigation).WithMany()
                .HasForeignKey(d => d.GeoRegion)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("sla_policies_geo_region_fkey");

            entity.HasOne(d => d.TierCodeNavigation).WithMany()
                .HasForeignKey(d => d.TierCode)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("sla_policies_tier_code_fkey");

            entity.HasOne(d => d.UpdatedByNavigation).WithMany()
                .HasForeignKey(d => d.UpdatedBy)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("sla_policies_updated_by_fkey");
        });

        modelBuilder.Entity<SlaTicket>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("sla_tickets_pkey");

            entity.ToTable("sla_tickets", "sla", tb => tb.HasComment("Per-ticket SLA clock. One-to-one with tickets. paused_minutes accumulates total pause time from On Hold / Product Roadmap / Pending states. resolution_due_at is extended on resume. Breach flags set by SLA engine daemon / scheduled job."));

            entity.HasIndex(e => e.FollowUpDueAt, "idx_sla_follow_up_due").HasFilter("((follow_up_at IS NULL) AND (follow_up_due_at IS NOT NULL))");

            entity.HasIndex(e => new { e.FirstResponseBreached, e.ResolutionBreached }, "idx_sla_tickets_breached");

            entity.HasIndex(e => e.ResolutionDueAt, "idx_sla_tickets_due").HasFilter("(resolved_at IS NULL)");

            entity.HasIndex(e => e.TicketId, "uq_sla_tickets_ticket").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.FirstResponseAt).HasColumnName("first_response_at");
            entity.Property(e => e.FirstResponseBreached).HasColumnName("first_response_breached");
            entity.Property(e => e.FirstResponseDueAt).HasColumnName("first_response_due_at");
            entity.Property(e => e.FollowUpAt).HasColumnName("follow_up_at");
            entity.Property(e => e.FollowUpBreached)
                .HasComment("TRUE when follow_up_at IS NULL AND NOW() > follow_up_due_at. Set by SLA engine daemon alongside first_response_breached and resolution_breached.")
                .HasColumnName("follow_up_breached");
            entity.Property(e => e.FollowUpDueAt)
                .HasComment("Third SLA stage: follow-up deadline. Typically set when ticket enters pending_customer status — agent must follow up if no customer response within N business hours. Prevents stale pending tickets.")
                .HasColumnName("follow_up_due_at");
            entity.Property(e => e.LastPausedAt).HasColumnName("last_paused_at");
            entity.Property(e => e.PausedMinutes).HasColumnName("paused_minutes");
            entity.Property(e => e.PolicyId).HasColumnName("policy_id");
            entity.Property(e => e.ResolutionBreached).HasColumnName("resolution_breached");
            entity.Property(e => e.ResolutionDueAt).HasColumnName("resolution_due_at");
            entity.Property(e => e.ResolvedAt).HasColumnName("resolved_at");
            entity.Property(e => e.TicketId).HasColumnName("ticket_id");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Policy).WithMany(p => p.SlaTickets)
                .HasForeignKey(d => d.PolicyId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("sla_tickets_policy_id_fkey");

            entity.HasOne(d => d.Ticket).WithOne(p => p.SlaTicket)
                .HasForeignKey<SlaTicket>(d => d.TicketId)
                .HasConstraintName("sla_tickets_ticket_id_fkey");
        });

        // SolutionType configuration is loaded from Configurations

        modelBuilder.Entity<StatusTransition>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("status_transitions_pkey");

            entity.ToTable("status_transitions", "workflow", tb => tb.HasComment("One row per allowed edge in a graph. API layer validates every status change against this table. requires_field check: named tickets column must be non-null before transition allowed. auto_trigger=true allows automation engine to fire without human actor."));

            entity.HasIndex(e => new { e.GraphId, e.FromStatus }, "idx_status_transitions_from").HasFilter("(is_deleted = false)");

            entity.HasIndex(e => e.GraphId, "idx_status_transitions_graph").HasFilter("(is_deleted = false)");

            entity.HasIndex(e => new { e.GraphId, e.FromStatus, e.ToStatus }, "uq_status_transitions").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.AutoTrigger).HasColumnName("auto_trigger");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.DisplayOrder).HasColumnName("display_order");
            entity.Property(e => e.FromStatus)
                .HasMaxLength(40)
                .HasColumnName("from_status");
            entity.Property(e => e.GraphId).HasColumnName("graph_id");
            entity.Property(e => e.IsDeleted).HasColumnName("is_deleted");
            entity.Property(e => e.RequiresField)
                .HasMaxLength(100)
                .HasColumnName("requires_field");
            entity.Property(e => e.RequiresRole)
                .HasMaxLength(80)
                .HasColumnName("requires_role");
            entity.Property(e => e.ToStatus)
                .HasMaxLength(40)
                .HasColumnName("to_status");

            entity.HasOne(d => d.CreatedByNavigation).WithMany()
                .HasForeignKey(d => d.CreatedBy)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("status_transitions_created_by_fkey");

            entity.HasOne(d => d.Graph).WithMany()
                .HasForeignKey(d => d.GraphId)
                .HasConstraintName("status_transitions_graph_id_fkey");

            entity.HasOne(d => d.RequiresRoleNavigation).WithMany()
                .HasPrincipalKey(p => p.Name)
                .HasForeignKey(d => d.RequiresRole)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("status_transitions_requires_role_fkey");
        });

        // SubModule configuration is loaded from Configurations

        modelBuilder.Entity<Ticket>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("tickets_pkey");

            entity.ToTable("tickets", "ticket", tb => tb.HasComment("Central transactional entity. graph_id is resolved once at creation via scope engine. version_id/module_id/sub_module_id are normalized FK refs per Addendum v7. sla_excluded=true during hypercare/delivery mode. is_on_hold_payment for payment holds."));

            entity.HasIndex(e => new { e.IsAutoResolved, e.CreatedAt }, "idx_tickets_auto_resolved").HasFilter("((is_auto_resolved = true) AND (is_deleted = false))");

            entity.HasIndex(e => e.CreatedAt, "idx_tickets_created_at")
                .IsDescending()
                .HasFilter("(is_deleted = false)");

            entity.HasIndex(e => e.ForceP1, "idx_tickets_force_p1").HasFilter("((force_p1 = true) AND (is_deleted = false))");

            entity.HasIndex(e => e.IsOnHoldPayment, "idx_tickets_payment_hold").HasFilter("((is_on_hold_payment = true) AND (is_deleted = false))");

            entity.HasIndex(e => e.PriorityScore, "idx_tickets_priority_score")
                .IsDescending()
                .HasFilter("(is_deleted = false)")
                .HasNullSortOrder(new[] { NullSortOrder.NullsLast });

            entity.HasIndex(e => e.SolutionTypeId, "idx_tickets_solution_type").HasFilter("((solution_type_id IS NOT NULL) AND (is_deleted = false))");

            entity.HasIndex(e => e.TicketNumber, "idx_tickets_ticket_number").IsUnique();

            entity.HasIndex(e => e.VersionId, "idx_tickets_version").HasFilter("(is_deleted = false)");

            entity.HasIndex(e => e.TicketNumber, "uq_tickets_number").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.AssignedAgentId).HasColumnName("assigned_agent_id");
            entity.Property(e => e.AuditNotes).HasColumnName("audit_notes");
            entity.Property(e => e.AuditedBy).HasColumnName("audited_by");
            entity.Property(e => e.CompanyId).HasColumnName("company_id");
            entity.Property(e => e.ContactId).HasColumnName("contact_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.CreatedByUserId).HasColumnName("created_by_user_id");
            entity.Property(e => e.CustomerCallTaken).HasColumnName("customer_call_taken");
            entity.Property(e => e.CustomerReplyCount)
                .HasComment("Incremented by automation rule on every inbound customer comment. Re-evaluation loop: every increment triggers risk agent re-score. Multiple follow-ups (≥3) increase urgency_score by +1 in the scoring formula.")
                .HasColumnName("customer_reply_count");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.FixType)
                .HasMaxLength(20)
                .HasColumnName("fix_type");
            entity.Property(e => e.ForceP1)
                .HasComment("TRUE when a force-P1 rule fires: system_down, security_breach, or sla_breach_imminent. Overrides the computed priority_score regardless of value.")
                .HasColumnName("force_p1");
            entity.Property(e => e.GraphId).HasColumnName("graph_id");
            entity.Property(e => e.GroupId).HasColumnName("group_id");
            entity.Property(e => e.ImpactScore)
                .HasPrecision(4, 2)
                .HasColumnName("impact_score");
            entity.Property(e => e.IsAutoResolved)
                .HasComment("TRUE when the auto-resolution engine closed this ticket without human intervention. Used for KPI: target is 30–40% of total tickets. Set by auto-resolution engine after confidence > 0.85 match and solution sent.")
                .HasColumnName("is_auto_resolved");
            entity.Property(e => e.IsDeleted).HasColumnName("is_deleted");
            entity.Property(e => e.IsOnHoldPayment).HasColumnName("is_on_hold_payment");
            entity.Property(e => e.LinkedJiraId)
                .HasMaxLength(100)
                .HasColumnName("linked_jira_id");
            entity.Property(e => e.ModifiedAt).HasColumnName("modified_at");
            entity.Property(e => e.ModifiedBy)
                .HasMaxLength(100)
                .HasColumnName("modified_by");
            entity.Property(e => e.ModuleId).HasColumnName("module_id");
            entity.Property(e => e.PriorityScore)
                .HasPrecision(4, 2)
                .HasComment("Computed weighted priority score (0–5). Formula: (0.30×impact) + (0.20×urgency) + (0.15×sentiment) + (0.15×sla_severity) + (0.10×type) + (0.10×tier). Mapped: ≥4.5=P1(Urgent), 3.5–4.49=P2(High), 2.5–3.49=P3(Medium), <2.5=P4(Low).")
                .HasColumnName("priority_score");
            entity.Property(e => e.PriorityScoreAt)
                .HasComment("Timestamp of last priority score computation. Re-evaluation loop updates this on every customer reply or SLA status change.")
                .HasColumnName("priority_score_at");
            entity.Property(e => e.ProductType)
                .HasMaxLength(40)
                .HasColumnName("product_type");
            entity.Property(e => e.Rca).HasColumnName("rca");
            entity.Property(e => e.SentimentScore)
                .HasPrecision(4, 2)
                .HasColumnName("sentiment_score");
            entity.Property(e => e.SlaExcluded).HasColumnName("sla_excluded");
            entity.Property(e => e.SlaSeverityScore)
                .HasPrecision(4, 2)
                .HasColumnName("sla_severity_score");
            entity.Property(e => e.SolutionType)
                .HasMaxLength(40)
                .HasColumnName("solution_type");
            entity.Property(e => e.SolutionTypeId)
                .HasComment("FK to lookup.solution_types. Replaces the free-text solution_type varchar column. Both columns kept during migration; remove solution_type varchar after data backfill.")
                .HasColumnName("solution_type_id");
            entity.Property(e => e.SubModuleId).HasColumnName("sub_module_id");
            entity.Property(e => e.Subject).HasColumnName("subject");
            entity.Property(e => e.TicketNumber)
                .HasMaxLength(20)
                .HasColumnName("ticket_number");
            entity.Property(e => e.TierWeight)
                .HasPrecision(4, 2)
                .HasColumnName("tier_weight");
            entity.Property(e => e.TypeWeight)
                .HasPrecision(4, 2)
                .HasColumnName("type_weight");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("updated_at");
            entity.Property(e => e.UpdatedBy).HasColumnName("updated_by");
            entity.Property(e => e.UrgencyScore)
                .HasPrecision(4, 2)
                .HasColumnName("urgency_score");
            entity.Property(e => e.VersionId).HasColumnName("version_id");

            entity.HasOne(d => d.AssignedAgent).WithMany()
                .HasForeignKey(d => d.AssignedAgentId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("tickets_assigned_agent_id_fkey");

            entity.HasOne(d => d.AuditedByNavigation).WithMany()
                .HasForeignKey(d => d.AuditedBy)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("tickets_audited_by_fkey");

            entity.HasOne(d => d.Company).WithMany()
                .HasForeignKey(d => d.CompanyId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("tickets_company_id_fkey");

            entity.HasOne(d => d.Contact).WithMany()
                .HasForeignKey(d => d.ContactId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("tickets_contact_id_fkey");

            entity.HasOne(d => d.CreatedByNavigation).WithMany()
                .HasForeignKey(d => d.CreatedBy)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("tickets_created_by_fkey");

            entity.HasOne(d => d.CreatedByUser).WithMany()
                .HasForeignKey(d => d.CreatedByUserId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("tickets_created_by_user_id_fkey");

            entity.HasOne(d => d.Graph).WithMany()
                .HasForeignKey(d => d.GraphId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("tickets_graph_id_fkey");

            entity.HasOne(d => d.Group).WithMany()
                .HasForeignKey(d => d.GroupId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("tickets_group_id_fkey");

            entity.HasOne(d => d.Module).WithMany()
                .HasForeignKey(d => d.ModuleId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("tickets_module_id_fkey");

            entity.HasOne(d => d.SolutionTypeNavigation).WithMany()
                .HasForeignKey(d => d.SolutionTypeId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("tickets_solution_type_id_fkey");

            entity.HasOne(d => d.SubModule).WithMany()
                .HasForeignKey(d => d.SubModuleId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("tickets_sub_module_id_fkey");

            entity.HasOne(d => d.UpdatedByNavigation).WithMany()
                .HasForeignKey(d => d.UpdatedBy)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("tickets_updated_by_fkey");

            entity.HasOne(d => d.Version).WithMany()
                .HasForeignKey(d => d.VersionId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("tickets_version_id_fkey");
        });

        modelBuilder.Entity<TicketAssignmentLog>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("ticket_assignment_log_pkey");

            entity.ToTable("ticket_assignment_log", "ticket");

            entity.HasIndex(e => new { e.ToAgentId, e.AssignedAt }, "idx_tal_agent").IsDescending(false, true);

            entity.HasIndex(e => new { e.TicketId, e.AssignedAt }, "idx_tal_ticket").IsDescending(false, true);

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.AssignedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("assigned_at");
            entity.Property(e => e.ChangedBy).HasColumnName("changed_by");
            entity.Property(e => e.FromAgentId).HasColumnName("from_agent_id");
            entity.Property(e => e.Notes).HasColumnName("notes");
            entity.Property(e => e.TicketId).HasColumnName("ticket_id");
            entity.Property(e => e.ToAgentId).HasColumnName("to_agent_id");

            entity.HasOne(d => d.ChangedByNavigation).WithMany()
                .HasForeignKey(d => d.ChangedBy)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("ticket_assignment_log_changed_by_fkey");

            entity.HasOne(d => d.FromAgent).WithMany()
                .HasForeignKey(d => d.FromAgentId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("ticket_assignment_log_from_agent_id_fkey");

            entity.HasOne(d => d.Ticket).WithMany(p => p.TicketAssignmentLogs)
                .HasForeignKey(d => d.TicketId)
                .HasConstraintName("ticket_assignment_log_ticket_id_fkey");

            entity.HasOne(d => d.ToAgent).WithMany()
                .HasForeignKey(d => d.ToAgentId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("ticket_assignment_log_to_agent_id_fkey");
        });

        modelBuilder.Entity<TicketAttachment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("ticket_attachments_pkey");

            entity.ToTable("ticket_attachments", "ticket");

            entity.HasIndex(e => e.CommentId, "idx_ticket_attachments_comment").HasFilter("((comment_id IS NOT NULL) AND (is_deleted = false))");

            entity.HasIndex(e => e.TicketId, "idx_ticket_attachments_ticket").HasFilter("(is_deleted = false)");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.CommentId).HasColumnName("comment_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.FileName)
                .HasMaxLength(255)
                .HasColumnName("file_name");
            entity.Property(e => e.FileSizeBytes).HasColumnName("file_size_bytes");
            entity.Property(e => e.FileUrl).HasColumnName("file_url");
            entity.Property(e => e.IsDeleted).HasColumnName("is_deleted");
            entity.Property(e => e.MimeType)
                .HasMaxLength(100)
                .HasColumnName("mime_type");
            entity.Property(e => e.TicketId).HasColumnName("ticket_id");

            entity.HasOne(d => d.Comment).WithMany()
                .HasForeignKey(d => d.CommentId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("ticket_attachments_comment_id_fkey");

            entity.HasOne(d => d.CreatedByNavigation).WithMany()
                .HasForeignKey(d => d.CreatedBy)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("ticket_attachments_created_by_fkey");

            entity.HasOne(d => d.Ticket).WithMany()
                .HasForeignKey(d => d.TicketId)
                .HasConstraintName("ticket_attachments_ticket_id_fkey");
        });

        modelBuilder.Entity<TicketClassification>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("ticket_classification_pkey");

            entity.ToTable("ticket_classification", "ai", tb => tb.HasComment("One row per ticket — full C-R-L (Classifier-Retrieval-LLM) pipeline audit. classifier_auto_routed=TRUE means phase 2+3 were skipped (confidence ≥ threshold). retrieval_discrepancy=TRUE triggered Phase 3 LLM. llm_invoked tracks LLM cost exposure. model_version enables A/B testing."));

            entity.HasIndex(e => new { e.FinalLabel, e.ClassifiedAt }, "idx_ticket_classification_label").IsDescending(false, true);

            entity.HasIndex(e => new { e.LlmInvoked, e.ClassifiedAt }, "idx_ticket_classification_llm")
                .IsDescending(false, true)
                .HasFilter("(llm_invoked = true)");

            entity.HasIndex(e => e.TicketId, "idx_ticket_classification_ticket");

            entity.HasIndex(e => e.TicketId, "uq_ticket_classification_ticket").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.ClassifiedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("classified_at");
            entity.Property(e => e.ClassifierAutoRouted).HasColumnName("classifier_auto_routed");
            entity.Property(e => e.ClassifierConfidence)
                .HasPrecision(5, 4)
                .HasColumnName("classifier_confidence");
            entity.Property(e => e.ClassifierLabel)
                .HasMaxLength(60)
                .HasColumnName("classifier_label");
            entity.Property(e => e.FinalConfidence)
                .HasPrecision(5, 4)
                .HasColumnName("final_confidence");
            entity.Property(e => e.FinalLabel)
                .HasMaxLength(60)
                .HasColumnName("final_label");
            entity.Property(e => e.LlmFinalLabel)
                .HasMaxLength(60)
                .HasColumnName("llm_final_label");
            entity.Property(e => e.LlmInvoked).HasColumnName("llm_invoked");
            entity.Property(e => e.LlmReasoning).HasColumnName("llm_reasoning");
            entity.Property(e => e.ModelVersion)
                .HasMaxLength(40)
                .HasColumnName("model_version");
            entity.Property(e => e.RetrievalConsensusLabel)
                .HasMaxLength(60)
                .HasColumnName("retrieval_consensus_label");
            entity.Property(e => e.RetrievalDiscrepancy).HasColumnName("retrieval_discrepancy");
            entity.Property(e => e.RetrievalTopKLabels)
                .HasColumnType("jsonb")
                .HasColumnName("retrieval_top_k_labels");
            entity.Property(e => e.TicketId).HasColumnName("ticket_id");

            entity.HasOne(d => d.Ticket).WithOne(p => p.TicketClassification)
                .HasForeignKey<TicketClassification>(d => d.TicketId)
                .HasConstraintName("ticket_classification_ticket_id_fkey");
        });

        modelBuilder.Entity<TicketComment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("ticket_comments_pkey");

            entity.ToTable("ticket_comments", "ticket");

            entity.HasIndex(e => e.AuthorId, "idx_ticket_comments_author").HasFilter("(is_deleted = false)");

            entity.HasIndex(e => new { e.TicketId, e.CreatedAt }, "idx_ticket_comments_ticket").HasFilter("(is_deleted = false)");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.AuthorId).HasColumnName("author_id");
            entity.Property(e => e.Body).HasColumnName("body");
            entity.Property(e => e.ContactId).HasColumnName("contact_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.IsDeleted).HasColumnName("is_deleted");
            entity.Property(e => e.IsPrivate).HasColumnName("is_private");
            entity.Property(e => e.TicketId).HasColumnName("ticket_id");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Author).WithMany()
                .HasForeignKey(d => d.AuthorId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("ticket_comments_author_id_fkey");

            entity.HasOne(d => d.Contact).WithMany(p => p.TicketComments)
                .HasForeignKey(d => d.ContactId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("ticket_comments_contact_id_fkey");

            entity.HasOne(d => d.CreatedByNavigation).WithMany()
                .HasForeignKey(d => d.CreatedBy)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("ticket_comments_created_by_fkey");

            entity.HasOne(d => d.Ticket).WithMany(p => p.TicketComments)
                .HasForeignKey(d => d.TicketId)
                .HasConstraintName("ticket_comments_ticket_id_fkey");
        });

        modelBuilder.Entity<TicketCustomField>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("ticket_custom_fields_pkey");

            entity.ToTable("ticket_custom_fields", "ticket");

            entity.HasIndex(e => new { e.FieldKey, e.FieldValue }, "idx_tcf_key");

            entity.HasIndex(e => e.TicketId, "idx_tcf_ticket");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.FieldKey)
                .HasMaxLength(80)
                .HasColumnName("field_key");
            entity.Property(e => e.FieldValue).HasColumnName("field_value");
            entity.Property(e => e.TicketId).HasColumnName("ticket_id");

            entity.HasOne(d => d.Ticket).WithMany(p => p.TicketCustomFields)
                .HasForeignKey(d => d.TicketId)
                .HasConstraintName("ticket_custom_fields_ticket_id_fkey");
        });

        modelBuilder.Entity<TicketRiskScore>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("ticket_risk_scores_pkey");

            entity.ToTable("ticket_risk_scores", "ai", tb => tb.HasComment("Full audit of every priority score computation run. Multiple rows per ticket because the re-evaluation loop fires on every customer reply and SLA status change. trigger_event shows what caused the re-score. customer_reply_count_at enables auditing how many replies pushed priority up."));

            entity.HasIndex(e => new { e.ForceP1Triggered, e.ComputedAt }, "idx_risk_scores_force_p1")
                .IsDescending(false, true)
                .HasFilter("(force_p1_triggered = true)");

            entity.HasIndex(e => new { e.FinalScore, e.ComputedAt }, "idx_risk_scores_high_priority")
                .IsDescending()
                .HasFilter("(final_score >= 3.5)");

            entity.HasIndex(e => new { e.TicketId, e.ComputedAt }, "idx_risk_scores_ticket").IsDescending(false, true);

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.AssignedPriority)
                .HasMaxLength(20)
                .HasColumnName("assigned_priority");
            entity.Property(e => e.BusinessCriticalBump).HasColumnName("business_critical_bump");
            entity.Property(e => e.CompetitorThreatFlag).HasColumnName("competitor_threat_flag");
            entity.Property(e => e.ComputedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("computed_at");
            entity.Property(e => e.CustomerReplyCountAt).HasColumnName("customer_reply_count_at");
            entity.Property(e => e.FinalScore)
                .HasPrecision(4, 2)
                .HasColumnName("final_score");
            entity.Property(e => e.ForceP1Reason)
                .HasMaxLength(100)
                .HasColumnName("force_p1_reason");
            entity.Property(e => e.ForceP1Triggered).HasColumnName("force_p1_triggered");
            entity.Property(e => e.ImpactScore)
                .HasPrecision(4, 2)
                .HasColumnName("impact_score");
            entity.Property(e => e.ModulesAffectedScore)
                .HasPrecision(4, 2)
                .HasColumnName("modules_affected_score");
            entity.Property(e => e.PilotProjectFlag).HasColumnName("pilot_project_flag");
            entity.Property(e => e.SentimentLabel)
                .HasMaxLength(30)
                .HasColumnName("sentiment_label");
            entity.Property(e => e.SentimentScore)
                .HasPrecision(4, 2)
                .HasColumnName("sentiment_score");
            entity.Property(e => e.SlaSeverityScore)
                .HasPrecision(4, 2)
                .HasColumnName("sla_severity_score");
            entity.Property(e => e.TicketId).HasColumnName("ticket_id");
            entity.Property(e => e.TierWeight)
                .HasPrecision(4, 2)
                .HasColumnName("tier_weight");
            entity.Property(e => e.TriggerEvent)
                .HasMaxLength(60)
                .HasColumnName("trigger_event");
            entity.Property(e => e.TypeWeight)
                .HasPrecision(4, 2)
                .HasColumnName("type_weight");
            entity.Property(e => e.UrgencyKeywords).HasColumnName("urgency_keywords");
            entity.Property(e => e.UrgencyScore)
                .HasPrecision(4, 2)
                .HasColumnName("urgency_score");
            entity.Property(e => e.UsersAffectedScore)
                .HasPrecision(4, 2)
                .HasColumnName("users_affected_score");

            entity.HasOne(d => d.Ticket).WithMany(p => p.TicketRiskScores)
                .HasForeignKey(d => d.TicketId)
                .HasConstraintName("ticket_risk_scores_ticket_id_fkey");
        });

        modelBuilder.Entity<TicketStatusGraph>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("ticket_status_graphs_pkey");

            entity.ToTable("ticket_status_graphs", "workflow", tb => tb.HasComment("Named status transition graphs. graph_id is resolved once at ticket creation via ticket_status_graph_scopes priority engine and stored on tickets.graph_id. Subsequent transitions are validated against this graph_id only."));

            entity.HasIndex(e => e.IsActive, "idx_tsg_active").HasFilter("(is_deleted = false)");

            entity.HasIndex(e => e.GraphCode, "uq_ticket_status_graphs_code").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.GraphCode)
                .HasMaxLength(40)
                .HasColumnName("graph_code");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.IsDeleted).HasColumnName("is_deleted");
            entity.Property(e => e.Name)
                .HasMaxLength(150)
                .HasColumnName("name");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("updated_at");
            entity.Property(e => e.UpdatedBy).HasColumnName("updated_by");

            entity.HasOne(d => d.CreatedByNavigation).WithMany()
                .HasForeignKey(d => d.CreatedBy)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("ticket_status_graphs_created_by_fkey");

            entity.HasOne(d => d.UpdatedByNavigation).WithMany()
                .HasForeignKey(d => d.UpdatedBy)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("ticket_status_graphs_updated_by_fkey");
        });

        modelBuilder.Entity<TicketStatusGraphScope>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("ticket_status_graph_scopes_pkey");

            entity.ToTable("ticket_status_graph_scopes", "workflow", tb => tb.HasComment("Scope-to-graph mapping. NULL dimensions act as wildcards. Resolver evaluates rows by priority DESC and returns the first match. GRAPH-001 (priority=0) is the default fallback. Uses FK references to product_versions, modules, sub_modules — replaces old varchar fields."));

            entity.HasIndex(e => e.GraphId, "idx_tsgs_graph").HasFilter("(is_deleted = false)");

            entity.HasIndex(e => new { e.Priority, e.VersionId, e.ModuleId, e.SubModuleId }, "idx_tsgs_resolution")
                .IsDescending(true, false, false, false)
                .HasFilter("((is_active = true) AND (is_deleted = false))");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.Department)
                .HasMaxLength(80)
                .HasColumnName("department");
            entity.Property(e => e.GraphId).HasColumnName("graph_id");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.IsDeleted).HasColumnName("is_deleted");
            entity.Property(e => e.ModuleId).HasColumnName("module_id");
            entity.Property(e => e.Priority).HasColumnName("priority");
            entity.Property(e => e.SubModuleId).HasColumnName("sub_module_id");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("updated_at");
            entity.Property(e => e.UpdatedBy).HasColumnName("updated_by");
            entity.Property(e => e.VersionId).HasColumnName("version_id");

            entity.HasOne(d => d.CreatedByNavigation).WithMany()
                .HasForeignKey(d => d.CreatedBy)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("ticket_status_graph_scopes_created_by_fkey");

            entity.HasOne(d => d.Graph).WithMany()
                .HasForeignKey(d => d.GraphId)
                .HasConstraintName("ticket_status_graph_scopes_graph_id_fkey");

            entity.HasOne(d => d.Module).WithMany()
                .HasForeignKey(d => d.ModuleId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("ticket_status_graph_scopes_module_id_fkey");

            entity.HasOne(d => d.SubModule).WithMany()
                .HasForeignKey(d => d.SubModuleId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("ticket_status_graph_scopes_sub_module_id_fkey");

            entity.HasOne(d => d.UpdatedByNavigation).WithMany()
                .HasForeignKey(d => d.UpdatedBy)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("ticket_status_graph_scopes_updated_by_fkey");

            entity.HasOne(d => d.Version).WithMany()
                .HasForeignKey(d => d.VersionId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("ticket_status_graph_scopes_version_id_fkey");
        });

        modelBuilder.Entity<TicketStatusHistory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("ticket_status_history_pkey");

            entity.ToTable("ticket_status_history", "ticket", tb => tb.HasComment("Immutable audit log of every status transition. Never update or delete rows. Append-only; enables full status trail and SLA clock reconstruction."));

            entity.HasIndex(e => new { e.TicketId, e.ChangedAt }, "idx_tsh_ticket_time").IsDescending(false, true);

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.ChangedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("changed_at");
            entity.Property(e => e.ChangedBy).HasColumnName("changed_by");
            entity.Property(e => e.FromStatus)
                .HasMaxLength(40)
                .HasColumnName("from_status");
            entity.Property(e => e.Reason).HasColumnName("reason");
            entity.Property(e => e.TicketId).HasColumnName("ticket_id");
            entity.Property(e => e.ToStatus)
                .HasMaxLength(40)
                .HasColumnName("to_status");

            entity.HasOne(d => d.ChangedByNavigation).WithMany()
                .HasForeignKey(d => d.ChangedBy)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("ticket_status_history_changed_by_fkey");

            entity.HasOne(d => d.Ticket).WithMany()
                .HasForeignKey(d => d.TicketId)
                .HasConstraintName("ticket_status_history_ticket_id_fkey");
        });

        // TokenBlacklist, User, UserGroup, UserOtpCode, UserRole, UserSession, and UserVerificationToken configurations are loaded from Configurations

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AdrenalinDbContext).Assembly);

        modelBuilder.HasSequence("ticket_number_seq", "ticket");

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
