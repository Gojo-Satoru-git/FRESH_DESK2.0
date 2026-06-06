using System;
using System.Collections.Generic;
using System.Net;
using Adrenalin.Modules.Ticketing.Domain.Enums;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Adrenalin.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "gamification");

            migrationBuilder.EnsureSchema(
                name: "ai");

            migrationBuilder.EnsureSchema(
                name: "audit");

            migrationBuilder.EnsureSchema(
                name: "sla");

            migrationBuilder.EnsureSchema(
                name: "company");

            migrationBuilder.EnsureSchema(
                name: "ticket");

            migrationBuilder.EnsureSchema(
                name: "config");

            migrationBuilder.EnsureSchema(
                name: "lookup");

            migrationBuilder.EnsureSchema(
                name: "auth");

            migrationBuilder.EnsureSchema(
                name: "kb");

            migrationBuilder.EnsureSchema(
                name: "notification");

            migrationBuilder.EnsureSchema(
                name: "workflow");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:ai.agent_rating", "relevant,moderately_relevant,not_relevant")
                .Annotation("Npgsql:Enum:ai.suggestion_type", "troubleshooting_steps,similar_tickets")
                .Annotation("Npgsql:Enum:auth.revocation_reason", "logout,suspicious_reuse,admin_force,role_change")
                .Annotation("Npgsql:Enum:company.deployment_model", "cloud,on_premise,hybrid")
                .Annotation("Npgsql:Enum:gamification.badge_tier", "bronze,silver,gold,platinum")
                .Annotation("Npgsql:Enum:gamification.period_type", "daily,weekly,monthly,all_time")
                .Annotation("Npgsql:Enum:notification.delivery_status", "sent,failed,bounced,pending")
                .Annotation("Npgsql:Enum:notification.notification_channel", "email,in_app,sms")
                .Annotation("Npgsql:Enum:sla.trigger_event", "ticket_created,ticket_updated,time_based")
                .Annotation("Npgsql:Enum:ticket.ticket_priority", "urgent,high,medium,low")
                .Annotation("Npgsql:Enum:ticket.ticket_source", "email,portal,phone")
                .Annotation("Npgsql:Enum:ticket.ticket_status", "new,open,in_progress,pending_customer,pending_internal,on_hold,product_roadmap,pending_approval,compliance_review,dual_agent_confirm,resolved,reopened,closed")
                .Annotation("Npgsql:Enum:ticket.ticket_type", "bug,enhancement,incident,change_request,query,service_request,clarification,environment_issue")
                .Annotation("Npgsql:PostgresExtension:btree_gin", ",,")
                .Annotation("Npgsql:PostgresExtension:pg_trgm", ",,")
                .Annotation("Npgsql:PostgresExtension:pgcrypto", ",,")
                .Annotation("Npgsql:PostgresExtension:unaccent", ",,");

            migrationBuilder.CreateSequence(
                name: "ticket_number_seq",
                schema: "ticket");

            migrationBuilder.CreateTable(
                name: "customer_status_map",
                schema: "config",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    agent_status = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    customer_label = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    customer_description = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("customer_status_map_pkey", x => x.id);
                },
                comment: "Maps every agent-facing ticket status to the simplified customer-facing label. Customer portal API reads this table — customers never see statuses like pending_internal or compliance_review. Only Open, Reopen, Closed are exposed.");

            migrationBuilder.CreateTable(
                name: "customer_tiers",
                schema: "lookup",
                columns: table => new
                {
                    code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    label = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    priority_bump = table.Column<int>(type: "integer", nullable: false),
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("customer_tiers_pkey", x => x.code);
                },
                comment: "M1-M4 customer tiers. priority_bump is used by SLA engine to elevate priority at creation.");

            migrationBuilder.CreateTable(
                name: "geo_regions",
                schema: "lookup",
                columns: table => new
                {
                    code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    label = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    timezone = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    business_start = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    business_end = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    working_days = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValueSql: "'Mon-Fri'::character varying"),
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("geo_regions_pkey", x => x.code);
                },
                comment: "Region-specific business hours and timezone. SLA clock runs only within these windows.");

            migrationBuilder.CreateTable(
                name: "solution_types",
                schema: "lookup",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    code = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    label = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("solution_types_pkey", x => x.id);
                },
                comment: "Valid solution_type values: data_correction, patch_deployment, configuration, clarification, server_outage, ad_hoc, known_issue. Replaces free-text varchar on tickets.");

            migrationBuilder.CreateTable(
                name: "users",
                schema: "auth",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    normalized_email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    username = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    normalized_username = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    password_hash = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    first_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    last_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    phone = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    avatar_url = table.Column<string>(type: "text", nullable: true),
                    email_verified = table.Column<bool>(type: "boolean", nullable: false),
                    email_verified_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    password_changed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    failed_login_attempts = table.Column<int>(type: "integer", nullable: false),
                    lockout_end = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    last_login_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    row_version = table.Column<byte[]>(type: "bytea", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true, defaultValueSql: "now()"),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("users_pkey", x => x.id);
                    table.ForeignKey(
                        name: "users_created_by_fkey",
                        column: x => x.created_by,
                        principalSchema: "auth",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "users_updated_by_fkey",
                        column: x => x.updated_by,
                        principalSchema: "auth",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                },
                comment: "Core identity for all internal staff. Email uniqueness enforced case-insensitively. password_hash is bcrypt — never store plain. Soft-delete via is_deleted.");

            migrationBuilder.CreateTable(
                name: "agent_streaks",
                schema: "gamification",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    agent_id = table.Column<Guid>(type: "uuid", nullable: false),
                    current_streak_days = table.Column<int>(type: "integer", nullable: false),
                    longest_streak_days = table.Column<int>(type: "integer", nullable: false),
                    streak_start_date = table.Column<DateOnly>(type: "date", nullable: true),
                    last_activity_date = table.Column<DateOnly>(type: "date", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("agent_streaks_pkey", x => x.id);
                    table.ForeignKey(
                        name: "agent_streaks_agent_id_fkey",
                        column: x => x.agent_id,
                        principalSchema: "auth",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "One row per agent. current_streak_days = consecutive days with ≥1 SLA-met resolution. Nightly job resets if last_activity_date < CURRENT_DATE - 1 day.");

            migrationBuilder.CreateTable(
                name: "audit_log",
                schema: "audit",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    schema_name = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    table_name = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    record_id = table.Column<Guid>(type: "uuid", nullable: false),
                    action = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    old_values = table.Column<string>(type: "jsonb", nullable: true),
                    new_values = table.Column<string>(type: "jsonb", nullable: true),
                    changed_by = table.Column<Guid>(type: "uuid", nullable: true),
                    changed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    ip_address = table.Column<IPAddress>(type: "inet", nullable: true),
                    session_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("audit_log_pkey", x => x.id);
                    table.ForeignKey(
                        name: "audit_log_changed_by_fkey",
                        column: x => x.changed_by,
                        principalSchema: "auth",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                },
                comment: "Cross-schema forensic audit log. Populated by application layer or generic audit triggers. old_values/new_values store JSONB diff for immutable change history.");

            migrationBuilder.CreateTable(
                name: "automation_rules",
                schema: "sla",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    conditions = table.Column<string>(type: "jsonb", nullable: false, defaultValueSql: "'[]'::jsonb"),
                    actions = table.Column<string>(type: "jsonb", nullable: false, defaultValueSql: "'[]'::jsonb"),
                    execution_order = table.Column<int>(type: "integer", nullable: false, defaultValue: 100),
                    RowVersion = table.Column<byte[]>(type: "bytea", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true, defaultValueSql: "now()"),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("automation_rules_pkey", x => x.id);
                    table.ForeignKey(
                        name: "automation_rules_created_by_fkey",
                        column: x => x.created_by,
                        principalSchema: "auth",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "automation_rules_updated_by_fkey",
                        column: x => x.updated_by,
                        principalSchema: "auth",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                },
                comment: "JSONB conditions/actions allows rule engine to evolve without schema migrations. Examples: route to group on creation, reopen on reply, auto-close at day 15, escalate on no-response, set payment hold status. execution_order determines eval sequence.");

            migrationBuilder.CreateTable(
                name: "badges",
                schema: "gamification",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    code = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    icon_url = table.Column<string>(type: "text", nullable: true),
                    points_value = table.Column<int>(type: "integer", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("badges_pkey", x => x.id);
                    table.ForeignKey(
                        name: "badges_created_by_fkey",
                        column: x => x.created_by,
                        principalSchema: "auth",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "business_hours",
                schema: "sla",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    geo_region = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    start_time = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    end_time = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    working_days = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValueSql: "'Mon-Fri'::character varying"),
                    RowVersion = table.Column<byte[]>(type: "bytea", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("business_hours_pkey", x => x.id);
                    table.ForeignKey(
                        name: "business_hours_created_by_fkey",
                        column: x => x.created_by,
                        principalSchema: "auth",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "business_hours_geo_region_fkey",
                        column: x => x.geo_region,
                        principalSchema: "lookup",
                        principalTable: "geo_regions",
                        principalColumn: "code");
                });

            migrationBuilder.CreateTable(
                name: "companies",
                schema: "company",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    csp_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    geo_region = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    support_tier = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    industry = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    health_score = table.Column<int>(type: "integer", nullable: false, defaultValue: 75),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    is_payroll_customer = table.Column<bool>(type: "boolean", nullable: false),
                    leave_credit_cycle = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    migration_date = table.Column<DateOnly>(type: "date", nullable: true),
                    cam_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    delivery_manager_id = table.Column<Guid>(type: "uuid", nullable: true),
                    delivery_support_active = table.Column<bool>(type: "boolean", nullable: false),
                    payment_on_hold = table.Column<bool>(type: "boolean", nullable: false),
                    notes = table.Column<string>(type: "text", nullable: true),
                    modified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    modified_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    row_version = table.Column<byte[]>(type: "bytea", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true, defaultValueSql: "now()"),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("companies_pkey", x => x.id);
                    table.ForeignKey(
                        name: "companies_cam_user_id_fkey",
                        column: x => x.cam_user_id,
                        principalSchema: "auth",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "companies_created_by_fkey",
                        column: x => x.created_by,
                        principalSchema: "auth",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "companies_delivery_manager_id_fkey",
                        column: x => x.delivery_manager_id,
                        principalSchema: "auth",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "companies_geo_region_fkey",
                        column: x => x.geo_region,
                        principalSchema: "lookup",
                        principalTable: "geo_regions",
                        principalColumn: "code");
                    table.ForeignKey(
                        name: "companies_support_tier_fkey",
                        column: x => x.support_tier,
                        principalSchema: "lookup",
                        principalTable: "customer_tiers",
                        principalColumn: "code");
                    table.ForeignKey(
                        name: "companies_updated_by_fkey",
                        column: x => x.updated_by,
                        principalSchema: "auth",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                },
                comment: "Core customer account. health_score + customer_tiers.priority_bump elevates ticket priority. delivery_support_active routes tickets to delivery (hypercare). payment_on_hold triggers auto On Hold status for new tickets.");

            migrationBuilder.CreateTable(
                name: "escalation_rules",
                schema: "sla",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    no_response_minutes = table.Column<int>(type: "integer", nullable: false),
                    notify_role = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    RowVersion = table.Column<byte[]>(type: "bytea", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("escalation_rules_pkey", x => x.id);
                    table.ForeignKey(
                        name: "escalation_rules_created_by_fkey",
                        column: x => x.created_by,
                        principalSchema: "auth",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "groups",
                schema: "auth",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    region_code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    tier_code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    unattended_alert_minutes = table.Column<int>(type: "integer", nullable: false, defaultValue: 30, comment: "Minutes after which an unassigned ticket in this group triggers the GROUP_UNATTENDED notification to group leads. Default 30 minutes. Automation rule reads this value."),
                    row_version = table.Column<byte[]>(type: "bytea", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("groups_pkey", x => x.id);
                    table.ForeignKey(
                        name: "groups_created_by_fkey",
                        column: x => x.created_by,
                        principalSchema: "auth",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "groups_region_code_fkey",
                        column: x => x.region_code,
                        principalSchema: "lookup",
                        principalTable: "geo_regions",
                        principalColumn: "code");
                    table.ForeignKey(
                        name: "groups_tier_code_fkey",
                        column: x => x.tier_code,
                        principalSchema: "lookup",
                        principalTable: "customer_tiers",
                        principalColumn: "code");
                    table.ForeignKey(
                        name: "groups_updated_by_fkey",
                        column: x => x.updated_by,
                        principalSchema: "auth",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                },
                comment: "13 geo/tier groups. region_code and tier_code determine SLA policy and routing scope. Admin can create additional groups as org scales.");

            migrationBuilder.CreateTable(
                name: "holidays",
                schema: "sla",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    geo_region = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    holiday_date = table.Column<DateOnly>(type: "date", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    RowVersion = table.Column<byte[]>(type: "bytea", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("holidays_pkey", x => x.id);
                    table.ForeignKey(
                        name: "holidays_created_by_fkey",
                        column: x => x.created_by,
                        principalSchema: "auth",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "holidays_geo_region_fkey",
                        column: x => x.geo_region,
                        principalSchema: "lookup",
                        principalTable: "geo_regions",
                        principalColumn: "code");
                });

            migrationBuilder.CreateTable(
                name: "kb_folders",
                schema: "kb",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    parent_id = table.Column<Guid>(type: "uuid", nullable: true),
                    display_order = table.Column<int>(type: "integer", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("kb_folders_pkey", x => x.id);
                    table.ForeignKey(
                        name: "kb_folders_created_by_fkey",
                        column: x => x.created_by,
                        principalSchema: "auth",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "kb_folders_parent_id_fkey",
                        column: x => x.parent_id,
                        principalSchema: "kb",
                        principalTable: "kb_folders",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "kb_folders_updated_by_fkey",
                        column: x => x.updated_by,
                        principalSchema: "auth",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                },
                comment: "Self-referencing folder hierarchy. parent_id=NULL for root folders. Use WITH RECURSIVE CTE to retrieve full tree. Depth limit enforced in API layer.");

            migrationBuilder.CreateTable(
                name: "modules",
                schema: "lookup",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    code = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    label = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    department = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    is_mandatory_for_closure = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true, defaultValueSql: "now()"),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("modules_pkey", x => x.id);
                    table.ForeignKey(
                        name: "fk_modules_created_by",
                        column: x => x.created_by,
                        principalSchema: "auth",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_modules_updated_by",
                        column: x => x.updated_by,
                        principalSchema: "auth",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                },
                comment: "Top-level product functional areas. is_mandatory_for_closure enforces module selection before ticket resolution. department drives default graph scope lookup.");

            migrationBuilder.CreateTable(
                name: "notification_templates",
                schema: "notification",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    code = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    subject = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    body_html = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true, defaultValueSql: "now()"),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("notification_templates_pkey", x => x.id);
                    table.ForeignKey(
                        name: "notification_templates_created_by_fkey",
                        column: x => x.created_by,
                        principalSchema: "auth",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "notification_templates_updated_by_fkey",
                        column: x => x.updated_by,
                        principalSchema: "auth",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                },
                comment: "Handlebars templates for all notification events. code is the stable reference key. Examples: TICKET_CREATED, AGENT_REPLY, CSAT_SURVEY, SLA_BREACH, BADGE_AWARDED.");

            migrationBuilder.CreateTable(
                name: "permissions",
                schema: "auth",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    resource = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    action = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    row_version = table.Column<byte[]>(type: "bytea", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("permissions_pkey", x => x.id);
                    table.ForeignKey(
                        name: "permissions_created_by_fkey",
                        column: x => x.created_by,
                        principalSchema: "auth",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "permissions_updated_by_fkey",
                        column: x => x.updated_by,
                        principalSchema: "auth",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                },
                comment: "Atomic resource:action pairs. Examples: ticket:assign, company:create, ticket:delete. Permissions are system-defined and should not change without a migration.");

            migrationBuilder.CreateTable(
                name: "point_rules",
                schema: "gamification",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    code = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    points = table.Column<int>(type: "integer", nullable: false),
                    max_per_day = table.Column<int>(type: "integer", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("point_rules_pkey", x => x.id);
                    table.ForeignKey(
                        name: "point_rules_created_by_fkey",
                        column: x => x.created_by,
                        principalSchema: "auth",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                },
                comment: "Defines what actions earn points. max_per_day prevents farming: engine checks SUM(points) WHERE agent_id=X AND rule_id=Y AND earned_at::date = TODAY. Negative points allowed for reversals.");

            migrationBuilder.CreateTable(
                name: "portal_banners",
                schema: "kb",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    message = table.Column<string>(type: "text", nullable: false),
                    active_from = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    active_to = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("portal_banners_pkey", x => x.id);
                    table.ForeignKey(
                        name: "portal_banners_created_by_fkey",
                        column: x => x.created_by,
                        principalSchema: "auth",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "portal_banners_updated_by_fkey",
                        column: x => x.updated_by,
                        principalSchema: "auth",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "product_versions",
                schema: "lookup",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    label = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    release_date = table.Column<DateOnly>(type: "date", nullable: true),
                    is_legacy = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true, defaultValueSql: "now()"),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("product_versions_pkey", x => x.id);
                    table.ForeignKey(
                        name: "fk_product_versions_created_by",
                        column: x => x.created_by,
                        principalSchema: "auth",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_product_versions_updated_by",
                        column: x => x.updated_by,
                        principalSchema: "auth",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                },
                comment: "Named product release lines. code must be lowercase for scope resolver. Soft-deleted versions remain on historical tickets but are hidden from new-ticket UI.");

            migrationBuilder.CreateTable(
                name: "refresh_tokens",
                schema: "auth",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    token_hash = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    family_id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    device_info = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    ip_address = table.Column<string>(type: "text", nullable: true),
                    issued_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    expires_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    last_used_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    is_revoked = table.Column<bool>(type: "boolean", nullable: false),
                    revoked_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    RevokedReason = table.Column<int>(type: "integer", nullable: true),
                    replaced_by_token_id = table.Column<Guid>(type: "uuid", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "bytea", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("refresh_tokens_pkey", x => x.id);
                    table.ForeignKey(
                        name: "refresh_tokens_replaced_by_token_id_fkey",
                        column: x => x.replaced_by_token_id,
                        principalSchema: "auth",
                        principalTable: "refresh_tokens",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "refresh_tokens_user_id_fkey",
                        column: x => x.user_id,
                        principalSchema: "auth",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "Stores hashed refresh tokens with family-based rotation tracking. On token reuse detection (possible theft), entire family_id is revoked immediately. token_hash is SHA-256 of raw token.");

            migrationBuilder.CreateTable(
                name: "roles",
                schema: "auth",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    name = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    is_system_role = table.Column<bool>(type: "boolean", nullable: false),
                    row_version = table.Column<byte[]>(type: "bytea", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("roles_pkey", x => x.id);
                    table.UniqueConstraint("AK_roles_name", x => x.name);
                    table.ForeignKey(
                        name: "roles_created_by_fkey",
                        column: x => x.created_by,
                        principalSchema: "auth",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "roles_updated_by_fkey",
                        column: x => x.updated_by,
                        principalSchema: "auth",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                },
                comment: "Named roles: junior_agent, team_lead, manager, admin, collaborator, pmo. System roles (is_system_role=true) cannot be deleted via admin UI.");

            migrationBuilder.CreateTable(
                name: "sla_policies",
                schema: "sla",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    geo_region = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    tier_code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    priority = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    first_response_minutes = table.Column<int>(type: "integer", nullable: false),
                    resolution_minutes = table.Column<int>(type: "integer", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    RowVersion = table.Column<byte[]>(type: "bytea", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true, defaultValueSql: "now()"),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("sla_policies_pkey", x => x.id);
                    table.ForeignKey(
                        name: "sla_policies_created_by_fkey",
                        column: x => x.created_by,
                        principalSchema: "auth",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "sla_policies_geo_region_fkey",
                        column: x => x.geo_region,
                        principalSchema: "lookup",
                        principalTable: "geo_regions",
                        principalColumn: "code");
                    table.ForeignKey(
                        name: "sla_policies_tier_code_fkey",
                        column: x => x.tier_code,
                        principalSchema: "lookup",
                        principalTable: "customer_tiers",
                        principalColumn: "code");
                    table.ForeignKey(
                        name: "sla_policies_updated_by_fkey",
                        column: x => x.updated_by,
                        principalSchema: "auth",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                },
                comment: "One row per (geo_region × tier × priority). First response: Urgent=120m, High=240m, Med=360m, Low=480m. Resolution: Low=2880m (48 biz hrs). SLA minutes are business-hours only, not wall-clock.");

            migrationBuilder.CreateTable(
                name: "ticket_status_graphs",
                schema: "workflow",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    graph_code = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    row_version = table.Column<byte[]>(type: "bytea", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true, defaultValueSql: "now()"),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("ticket_status_graphs_pkey", x => x.id);
                    table.ForeignKey(
                        name: "ticket_status_graphs_created_by_fkey",
                        column: x => x.created_by,
                        principalSchema: "auth",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "ticket_status_graphs_updated_by_fkey",
                        column: x => x.updated_by,
                        principalSchema: "auth",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                },
                comment: "Named status transition graphs. graph_id is resolved once at ticket creation via ticket_status_graph_scopes priority engine and stored on tickets.graph_id. Subsequent transitions are validated against this graph_id only.");

            migrationBuilder.CreateTable(
                name: "token_blacklist",
                schema: "auth",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    jti = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    expires_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    reason = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    blacklisted_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false, defaultValueSql: "now()"),
                    RowVersion = table.Column<byte[]>(type: "bytea", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("token_blacklist_pkey", x => x.id);
                    table.ForeignKey(
                        name: "token_blacklist_user_id_fkey",
                        column: x => x.user_id,
                        principalSchema: "auth",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "Revoked JWT IDs. Auth middleware performs O(1) lookup on jti before accepting any token. Rows pruned nightly: DELETE FROM auth.token_blacklist WHERE expires_at < NOW().");

            migrationBuilder.CreateTable(
                name: "user_otp_codes",
                schema: "auth",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    code_hash = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    purpose = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    delivery_target = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    expires_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    verified_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    failed_attempts = table.Column<int>(type: "integer", nullable: false),
                    is_used = table.Column<bool>(type: "boolean", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "bytea", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("user_otp_codes_pkey", x => x.id);
                    table.ForeignKey(
                        name: "user_otp_codes_user_id_fkey",
                        column: x => x.user_id,
                        principalSchema: "auth",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "Hashed OTP codes for email/phone verification and 2FA. failed_attempts incremented on wrong guess; is_used=true on successful verification. Expired rows purged by nightly cleanup job.");

            migrationBuilder.CreateTable(
                name: "user_verification_tokens",
                schema: "auth",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    token_hash = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    purpose = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    expires_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    verified_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    is_used = table.Column<bool>(type: "boolean", nullable: false),
                    created_by_ip = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true),
                    RowVersion = table.Column<byte[]>(type: "bytea", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("user_verification_tokens_pkey", x => x.id);
                    table.ForeignKey(
                        name: "user_verification_tokens_user_id_fkey",
                        column: x => x.user_id,
                        principalSchema: "auth",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "Hashed URL tokens for email verification and password reset flows. is_used=true after first use (tokens are single-use). Expired rows purged by nightly cleanup job alongside refresh_tokens.");

            migrationBuilder.CreateTable(
                name: "company_contacts_limit",
                schema: "company",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    company_id = table.Column<Guid>(type: "uuid", nullable: false),
                    max_contacts = table.Column<int>(type: "integer", nullable: false, defaultValue: 10),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    row_version = table.Column<byte[]>(type: "bytea", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("company_contacts_limit_pkey", x => x.id);
                    table.ForeignKey(
                        name: "company_contacts_limit_company_id_fkey",
                        column: x => x.company_id,
                        principalSchema: "company",
                        principalTable: "companies",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "company_contacts_limit_created_by_fkey",
                        column: x => x.created_by,
                        principalSchema: "auth",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "company_domains",
                schema: "company",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    company_id = table.Column<Guid>(type: "uuid", nullable: false),
                    domain = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    is_primary = table.Column<bool>(type: "boolean", nullable: false),
                    created_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true, defaultValueSql: "'system'::character varying"),
                    row_version = table.Column<byte[]>(type: "bytea", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("company_domains_pkey", x => x.id);
                    table.ForeignKey(
                        name: "company_domains_company_id_fkey",
                        column: x => x.company_id,
                        principalSchema: "company",
                        principalTable: "companies",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "Email domains owned by each company. UNIQUE on lower(domain) enables O(1) auto-routing of incoming emails to the correct company account.");

            migrationBuilder.CreateTable(
                name: "contacts",
                schema: "company",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    company_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    phone = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    is_authorized = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    auto_created = table.Column<bool>(type: "boolean", nullable: false),
                    modified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    modified_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    row_version = table.Column<byte[]>(type: "bytea", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true, defaultValueSql: "now()"),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("contacts_pkey", x => x.id);
                    table.ForeignKey(
                        name: "contacts_company_id_fkey",
                        column: x => x.company_id,
                        principalSchema: "company",
                        principalTable: "companies",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "contacts_created_by_fkey",
                        column: x => x.created_by,
                        principalSchema: "auth",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "contacts_updated_by_fkey",
                        column: x => x.updated_by,
                        principalSchema: "auth",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "contacts_user_id_fkey",
                        column: x => x.user_id,
                        principalSchema: "auth",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                },
                comment: "Authorized contacts per company (5-20 per account). is_authorized=false blocks ticket creation. auto_created=true when system creates contact from unknown inbound email domain match.");

            migrationBuilder.CreateTable(
                name: "challenges",
                schema: "gamification",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    metric = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    target_value = table.Column<int>(type: "integer", nullable: false),
                    bonus_points = table.Column<int>(type: "integer", nullable: false),
                    badge_id = table.Column<Guid>(type: "uuid", nullable: true),
                    scope = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValueSql: "'global'::character varying"),
                    group_id = table.Column<Guid>(type: "uuid", nullable: true),
                    starts_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ends_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("challenges_pkey", x => x.id);
                    table.ForeignKey(
                        name: "challenges_badge_id_fkey",
                        column: x => x.badge_id,
                        principalSchema: "gamification",
                        principalTable: "badges",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "challenges_created_by_fkey",
                        column: x => x.created_by,
                        principalSchema: "auth",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "challenges_group_id_fkey",
                        column: x => x.group_id,
                        principalSchema: "auth",
                        principalTable: "groups",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "leaderboard_snapshots",
                schema: "gamification",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    agent_id = table.Column<Guid>(type: "uuid", nullable: false),
                    group_id = table.Column<Guid>(type: "uuid", nullable: true),
                    period_start = table.Column<DateOnly>(type: "date", nullable: false),
                    period_end = table.Column<DateOnly>(type: "date", nullable: false),
                    total_points = table.Column<int>(type: "integer", nullable: false),
                    tickets_resolved = table.Column<int>(type: "integer", nullable: false),
                    sla_met_count = table.Column<int>(type: "integer", nullable: false),
                    avg_csat = table.Column<decimal>(type: "numeric(4,2)", precision: 4, scale: 2, nullable: true),
                    rank = table.Column<int>(type: "integer", nullable: true),
                    computed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("leaderboard_snapshots_pkey", x => x.id);
                    table.ForeignKey(
                        name: "leaderboard_snapshots_agent_id_fkey",
                        column: x => x.agent_id,
                        principalSchema: "auth",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "leaderboard_snapshots_group_id_fkey",
                        column: x => x.group_id,
                        principalSchema: "auth",
                        principalTable: "groups",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                },
                comment: "Pre-computed daily/weekly/monthly leaderboards. Rebuilt at 00:05 by scheduled job. Reading live SUM on agent_points at request time is too expensive at scale.");

            migrationBuilder.CreateTable(
                name: "user_groups",
                schema: "auth",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    group_id = table.Column<Guid>(type: "uuid", nullable: false),
                    is_lead = table.Column<bool>(type: "boolean", nullable: false),
                    row_version = table.Column<byte[]>(type: "bytea", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("user_groups_pkey", x => x.id);
                    table.ForeignKey(
                        name: "user_groups_created_by_fkey",
                        column: x => x.created_by,
                        principalSchema: "auth",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "user_groups_group_id_fkey",
                        column: x => x.group_id,
                        principalSchema: "auth",
                        principalTable: "groups",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "user_groups_user_id_fkey",
                        column: x => x.user_id,
                        principalSchema: "auth",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "kb_articles",
                schema: "kb",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    title = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    content = table.Column<string>(type: "text", nullable: true),
                    article_type = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false, defaultValueSql: "'draft'::character varying"),
                    author_id = table.Column<Guid>(type: "uuid", nullable: true),
                    folder_id = table.Column<Guid>(type: "uuid", nullable: true),
                    is_published = table.Column<bool>(type: "boolean", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    auto_resolve = table.Column<bool>(type: "boolean", nullable: false, comment: "TRUE = this article is eligible for the auto-resolution engine. Engine only fires if confidence > confidence_threshold AND guardrail_excluded = FALSE."),
                    confidence_threshold = table.Column<decimal>(type: "numeric(4,3)", precision: 4, scale: 3, nullable: false, defaultValue: 0.850m, comment: "Minimum match confidence (0.85 default) required to trigger auto-resolve. Articles with high reopen rates should have this raised automatically by learning loop."),
                    keywords = table.Column<List<string>>(type: "text[]", nullable: true, comment: "Phase 1 (keyword match) trigger words. Stored as PostgreSQL text array. Example: ARRAY['forgot password', 'reset password', 'login failed']."),
                    resolution_text = table.Column<string>(type: "text", nullable: true),
                    guardrail_excluded = table.Column<bool>(type: "boolean", nullable: false, comment: "TRUE = this article covers a guardrail topic (payroll, financial, legal/compliance). Auto-resolution engine NEVER fires for guardrail_excluded articles regardless of confidence."),
                    times_matched = table.Column<int>(type: "integer", nullable: false, comment: "Learning loop counter: incremented each time this article is matched (auto-resolve attempted)."),
                    times_reopened = table.Column<int>(type: "integer", nullable: false, comment: "Learning loop counter: incremented each time a ticket auto-resolved via this article is reopened. High reopen rate → confidence_threshold auto-raised by learning loop job.")
                },
                constraints: table =>
                {
                    table.PrimaryKey("kb_articles_pkey", x => x.id);
                    table.ForeignKey(
                        name: "kb_articles_author_id_fkey",
                        column: x => x.author_id,
                        principalSchema: "auth",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "kb_articles_created_by_fkey",
                        column: x => x.created_by,
                        principalSchema: "auth",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "kb_articles_folder_id_fkey",
                        column: x => x.folder_id,
                        principalSchema: "kb",
                        principalTable: "kb_folders",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "kb_articles_updated_by_fkey",
                        column: x => x.updated_by,
                        principalSchema: "auth",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "sub_modules",
                schema: "lookup",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    module_id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    label = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    requires_dual_confirm = table.Column<bool>(type: "boolean", nullable: false),
                    requires_compliance_review = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true, defaultValueSql: "now()"),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("sub_modules_pkey", x => x.id);
                    table.ForeignKey(
                        name: "fk_sub_modules_created_by",
                        column: x => x.created_by,
                        principalSchema: "auth",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_sub_modules_updated_by",
                        column: x => x.updated_by,
                        principalSchema: "auth",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "sub_modules_module_id_fkey",
                        column: x => x.module_id,
                        principalSchema: "lookup",
                        principalTable: "modules",
                        principalColumn: "id");
                },
                comment: "Fine-grained ticket classification. requires_dual_confirm triggers GRAPH-006; requires_compliance_review triggers GRAPH-005. Sub-module wins over module in scope resolution.");

            migrationBuilder.CreateTable(
                name: "user_sessions",
                schema: "auth",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    refresh_token_id = table.Column<Guid>(type: "uuid", nullable: true),
                    device_name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    ip_address = table.Column<string>(type: "text", nullable: true),
                    geo_location = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    started_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    last_active_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    ended_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    RowVersion = table.Column<byte[]>(type: "bytea", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("user_sessions_pkey", x => x.id);
                    table.ForeignKey(
                        name: "user_sessions_refresh_token_id_fkey",
                        column: x => x.refresh_token_id,
                        principalSchema: "auth",
                        principalTable: "refresh_tokens",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "user_sessions_user_id_fkey",
                        column: x => x.user_id,
                        principalSchema: "auth",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "One row per device/login. Enables security dashboard with all active devices. last_active_at updated by API middleware on each authenticated call.");

            migrationBuilder.CreateTable(
                name: "role_permissions",
                schema: "auth",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    role_id = table.Column<Guid>(type: "uuid", nullable: false),
                    permission_id = table.Column<Guid>(type: "uuid", nullable: false),
                    row_version = table.Column<byte[]>(type: "bytea", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("role_permissions_pkey", x => x.id);
                    table.ForeignKey(
                        name: "role_permissions_created_by_fkey",
                        column: x => x.created_by,
                        principalSchema: "auth",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "role_permissions_permission_id_fkey",
                        column: x => x.permission_id,
                        principalSchema: "auth",
                        principalTable: "permissions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "role_permissions_role_id_fkey",
                        column: x => x.role_id,
                        principalSchema: "auth",
                        principalTable: "roles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "role_permissions_updated_by_fkey",
                        column: x => x.updated_by,
                        principalSchema: "auth",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "user_roles",
                schema: "auth",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    role_id = table.Column<Guid>(type: "uuid", nullable: false),
                    assigned_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    assigned_by = table.Column<Guid>(type: "uuid", nullable: true),
                    row_version = table.Column<byte[]>(type: "bytea", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("user_roles_pkey", x => x.id);
                    table.ForeignKey(
                        name: "user_roles_assigned_by_fkey",
                        column: x => x.assigned_by,
                        principalSchema: "auth",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "user_roles_created_by_fkey",
                        column: x => x.created_by,
                        principalSchema: "auth",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "user_roles_role_id_fkey",
                        column: x => x.role_id,
                        principalSchema: "auth",
                        principalTable: "roles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "user_roles_updated_by_fkey",
                        column: x => x.updated_by,
                        principalSchema: "auth",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "user_roles_user_id_fkey",
                        column: x => x.user_id,
                        principalSchema: "auth",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "status_transitions",
                schema: "workflow",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    graph_id = table.Column<Guid>(type: "uuid", nullable: false),
                    from_status = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    to_status = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    requires_role = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    requires_field = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    auto_trigger = table.Column<bool>(type: "boolean", nullable: false),
                    display_order = table.Column<int>(type: "integer", nullable: false),
                    row_version = table.Column<byte[]>(type: "bytea", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("status_transitions_pkey", x => x.id);
                    table.ForeignKey(
                        name: "status_transitions_created_by_fkey",
                        column: x => x.created_by,
                        principalSchema: "auth",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "status_transitions_graph_id_fkey",
                        column: x => x.graph_id,
                        principalSchema: "workflow",
                        principalTable: "ticket_status_graphs",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "status_transitions_requires_role_fkey",
                        column: x => x.requires_role,
                        principalSchema: "auth",
                        principalTable: "roles",
                        principalColumn: "name",
                        onDelete: ReferentialAction.Restrict);
                },
                comment: "One row per allowed edge in a graph. API layer validates every status change against this table. requires_field check: named tickets column must be non-null before transition allowed. auto_trigger=true allows automation engine to fire without human actor.");

            migrationBuilder.CreateTable(
                name: "agent_challenges",
                schema: "gamification",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    agent_id = table.Column<Guid>(type: "uuid", nullable: false),
                    challenge_id = table.Column<Guid>(type: "uuid", nullable: false),
                    current_value = table.Column<int>(type: "integer", nullable: false),
                    is_completed = table.Column<bool>(type: "boolean", nullable: false),
                    completed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    points_awarded = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("agent_challenges_pkey", x => x.id);
                    table.ForeignKey(
                        name: "agent_challenges_agent_id_fkey",
                        column: x => x.agent_id,
                        principalSchema: "auth",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "agent_challenges_challenge_id_fkey",
                        column: x => x.challenge_id,
                        principalSchema: "gamification",
                        principalTable: "challenges",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "kb_attachments",
                schema: "kb",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    article_id = table.Column<Guid>(type: "uuid", nullable: false),
                    file_url = table.Column<string>(type: "text", nullable: false),
                    file_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    file_size_bytes = table.Column<long>(type: "bigint", nullable: true),
                    mime_type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("kb_attachments_pkey", x => x.id);
                    table.ForeignKey(
                        name: "kb_attachments_article_id_fkey",
                        column: x => x.article_id,
                        principalSchema: "kb",
                        principalTable: "kb_articles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ticket_status_graph_scopes",
                schema: "workflow",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    graph_id = table.Column<Guid>(type: "uuid", nullable: false),
                    department = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    version_id = table.Column<Guid>(type: "uuid", nullable: true),
                    module_id = table.Column<Guid>(type: "uuid", nullable: true),
                    sub_module_id = table.Column<Guid>(type: "uuid", nullable: true),
                    priority = table.Column<int>(type: "integer", nullable: false),
                    row_version = table.Column<byte[]>(type: "bytea", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true, defaultValueSql: "now()"),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("ticket_status_graph_scopes_pkey", x => x.id);
                    table.ForeignKey(
                        name: "ticket_status_graph_scopes_created_by_fkey",
                        column: x => x.created_by,
                        principalSchema: "auth",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "ticket_status_graph_scopes_graph_id_fkey",
                        column: x => x.graph_id,
                        principalSchema: "workflow",
                        principalTable: "ticket_status_graphs",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "ticket_status_graph_scopes_module_id_fkey",
                        column: x => x.module_id,
                        principalSchema: "lookup",
                        principalTable: "modules",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "ticket_status_graph_scopes_sub_module_id_fkey",
                        column: x => x.sub_module_id,
                        principalSchema: "lookup",
                        principalTable: "sub_modules",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "ticket_status_graph_scopes_updated_by_fkey",
                        column: x => x.updated_by,
                        principalSchema: "auth",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "ticket_status_graph_scopes_version_id_fkey",
                        column: x => x.version_id,
                        principalSchema: "lookup",
                        principalTable: "product_versions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                },
                comment: "Scope-to-graph mapping. NULL dimensions act as wildcards. Resolver evaluates rows by priority DESC and returns the first match. GRAPH-001 (priority=0) is the default fallback. Uses FK references to product_versions, modules, sub_modules — replaces old varchar fields.");

            migrationBuilder.CreateTable(
                name: "tickets",
                schema: "ticket",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    ticket_number = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    company_id = table.Column<Guid>(type: "uuid", nullable: false),
                    contact_id = table.Column<Guid>(type: "uuid", nullable: true),
                    group_id = table.Column<Guid>(type: "uuid", nullable: true),
                    assigned_agent_id = table.Column<Guid>(type: "uuid", nullable: true),
                    created_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    graph_id = table.Column<Guid>(type: "uuid", nullable: true),
                    version_id = table.Column<Guid>(type: "uuid", nullable: true),
                    module_id = table.Column<Guid>(type: "uuid", nullable: false),
                    sub_module_id = table.Column<Guid>(type: "uuid", nullable: true),
                    subject = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: false),
                    status = table.Column<TicketStatus>(type: "ticket.ticket_status", nullable: false),
                    product_type = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    solution_type = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    fix_type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    rca = table.Column<string>(type: "text", nullable: true),
                    customer_call_taken = table.Column<bool>(type: "boolean", nullable: false),
                    linked_jira_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    audited_by = table.Column<Guid>(type: "uuid", nullable: true),
                    audit_notes = table.Column<string>(type: "text", nullable: true),
                    sla_excluded = table.Column<bool>(type: "boolean", nullable: false),
                    is_on_hold_payment = table.Column<bool>(type: "boolean", nullable: false),
                    solution_type_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "FK to lookup.solution_types. Replaces the free-text solution_type varchar column. Both columns kept during migration; remove solution_type varchar after data backfill."),
                    priority_score = table.Column<decimal>(type: "numeric(4,2)", precision: 4, scale: 2, nullable: true, comment: "Computed weighted priority score (0–5). Formula: (0.30×impact) + (0.20×urgency) + (0.15×sentiment) + (0.15×sla_severity) + (0.10×type) + (0.10×tier). Mapped: ≥4.5=P1(Urgent), 3.5–4.49=P2(High), 2.5–3.49=P3(Medium), <2.5=P4(Low)."),
                    impact_score = table.Column<decimal>(type: "numeric(4,2)", precision: 4, scale: 2, nullable: true),
                    urgency_score = table.Column<decimal>(type: "numeric(4,2)", precision: 4, scale: 2, nullable: true),
                    sentiment_score = table.Column<decimal>(type: "numeric(4,2)", precision: 4, scale: 2, nullable: true),
                    sla_severity_score = table.Column<decimal>(type: "numeric(4,2)", precision: 4, scale: 2, nullable: true),
                    type_weight = table.Column<decimal>(type: "numeric(4,2)", precision: 4, scale: 2, nullable: true),
                    tier_weight = table.Column<decimal>(type: "numeric(4,2)", precision: 4, scale: 2, nullable: true),
                    priority_score_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true, comment: "Timestamp of last priority score computation. Re-evaluation loop updates this on every customer reply or SLA status change."),
                    force_p1 = table.Column<bool>(type: "boolean", nullable: false, comment: "TRUE when a force-P1 rule fires: system_down, security_breach, or sla_breach_imminent. Overrides the computed priority_score regardless of value."),
                    is_auto_resolved = table.Column<bool>(type: "boolean", nullable: false, comment: "TRUE when the auto-resolution engine closed this ticket without human intervention. Used for KPI: target is 30–40% of total tickets. Set by auto-resolution engine after confidence > 0.85 match and solution sent."),
                    customer_reply_count = table.Column<int>(type: "integer", nullable: false, comment: "Incremented by automation rule on every inbound customer comment. Re-evaluation loop: every increment triggers risk agent re-score. Multiple follow-ups (≥3) increase urgency_score by +1 in the scoring formula."),
                    modified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    modified_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    row_version = table.Column<byte[]>(type: "bytea", rowVersion: true, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true, defaultValueSql: "now()"),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("tickets_pkey", x => x.id);
                    table.ForeignKey(
                        name: "tickets_assigned_agent_id_fkey",
                        column: x => x.assigned_agent_id,
                        principalSchema: "auth",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "tickets_audited_by_fkey",
                        column: x => x.audited_by,
                        principalSchema: "auth",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "tickets_company_id_fkey",
                        column: x => x.company_id,
                        principalSchema: "company",
                        principalTable: "companies",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "tickets_contact_id_fkey",
                        column: x => x.contact_id,
                        principalSchema: "company",
                        principalTable: "contacts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "tickets_created_by_fkey",
                        column: x => x.created_by,
                        principalSchema: "auth",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "tickets_created_by_user_id_fkey",
                        column: x => x.created_by_user_id,
                        principalSchema: "auth",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "tickets_graph_id_fkey",
                        column: x => x.graph_id,
                        principalSchema: "workflow",
                        principalTable: "ticket_status_graphs",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "tickets_group_id_fkey",
                        column: x => x.group_id,
                        principalSchema: "auth",
                        principalTable: "groups",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "tickets_module_id_fkey",
                        column: x => x.module_id,
                        principalSchema: "lookup",
                        principalTable: "modules",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "tickets_solution_type_id_fkey",
                        column: x => x.solution_type_id,
                        principalSchema: "lookup",
                        principalTable: "solution_types",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "tickets_sub_module_id_fkey",
                        column: x => x.sub_module_id,
                        principalSchema: "lookup",
                        principalTable: "sub_modules",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "tickets_updated_by_fkey",
                        column: x => x.updated_by,
                        principalSchema: "auth",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "tickets_version_id_fkey",
                        column: x => x.version_id,
                        principalSchema: "lookup",
                        principalTable: "product_versions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                },
                comment: "Central transactional entity. graph_id is resolved once at creation via scope engine. version_id/module_id/sub_module_id are normalized FK refs per Addendum v7. sla_excluded=true during hypercare/delivery mode. is_on_hold_payment for payment holds.");

            migrationBuilder.CreateTable(
                name: "agent_badges",
                schema: "gamification",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    agent_id = table.Column<Guid>(type: "uuid", nullable: false),
                    badge_id = table.Column<Guid>(type: "uuid", nullable: false),
                    ticket_id = table.Column<Guid>(type: "uuid", nullable: true),
                    earned_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    notified = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("agent_badges_pkey", x => x.id);
                    table.ForeignKey(
                        name: "agent_badges_agent_id_fkey",
                        column: x => x.agent_id,
                        principalSchema: "auth",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "agent_badges_badge_id_fkey",
                        column: x => x.badge_id,
                        principalSchema: "gamification",
                        principalTable: "badges",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "agent_badges_ticket_id_fkey",
                        column: x => x.ticket_id,
                        principalSchema: "ticket",
                        principalTable: "tickets",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "agent_points",
                schema: "gamification",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    agent_id = table.Column<Guid>(type: "uuid", nullable: false),
                    rule_id = table.Column<Guid>(type: "uuid", nullable: false),
                    ticket_id = table.Column<Guid>(type: "uuid", nullable: true),
                    points = table.Column<int>(type: "integer", nullable: false),
                    description = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    earned_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    earned_date = table.Column<DateOnly>(type: "date", nullable: false, defaultValueSql: "((now() AT TIME ZONE 'UTC'::text))::date"),
                    PointRuleId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("agent_points_pkey", x => x.id);
                    table.ForeignKey(
                        name: "FK_agent_points_point_rules_PointRuleId",
                        column: x => x.PointRuleId,
                        principalSchema: "gamification",
                        principalTable: "point_rules",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "agent_points_agent_id_fkey",
                        column: x => x.agent_id,
                        principalSchema: "auth",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "agent_points_rule_id_fkey",
                        column: x => x.rule_id,
                        principalSchema: "gamification",
                        principalTable: "point_rules",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "agent_points_ticket_id_fkey",
                        column: x => x.ticket_id,
                        principalSchema: "ticket",
                        principalTable: "tickets",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                },
                comment: "Immutable append-only point ledger. Event-sourcing pattern. Total = SUM(points). Enables full audit, point reversals (insert negative row), time-travel queries. Never update or delete — insert only.");

            migrationBuilder.CreateTable(
                name: "ai_suggestion_logs",
                schema: "ai",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    ticket_id = table.Column<Guid>(type: "uuid", nullable: false),
                    agent_id = table.Column<Guid>(type: "uuid", nullable: true),
                    suggestion_content = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("ai_suggestion_logs_pkey", x => x.id);
                    table.ForeignKey(
                        name: "ai_suggestion_logs_agent_id_fkey",
                        column: x => x.agent_id,
                        principalSchema: "auth",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "ai_suggestion_logs_ticket_id_fkey",
                        column: x => x.ticket_id,
                        principalSchema: "ticket",
                        principalTable: "tickets",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "AI suggestions shown to agent on new ticket. Two rows per ticket: troubleshooting_steps and similar_tickets. agent_rating feeds the ML training pipeline. ai_rating also stored redundantly on tickets.ai_rating for quick dashboard reporting.");

            migrationBuilder.CreateTable(
                name: "auto_resolution_log",
                schema: "ai",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    ticket_id = table.Column<Guid>(type: "uuid", nullable: false),
                    kb_article_id = table.Column<Guid>(type: "uuid", nullable: false),
                    match_phase = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    semantic_similarity = table.Column<decimal>(type: "numeric(5,4)", precision: 5, scale: 4, nullable: true),
                    final_confidence = table.Column<decimal>(type: "numeric(5,4)", precision: 5, scale: 4, nullable: false),
                    confidence_threshold = table.Column<decimal>(type: "numeric(5,4)", precision: 5, scale: 4, nullable: false),
                    action_taken = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    resolution_sent_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    resolution_channel = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    blocked_reason = table.Column<string>(type: "text", nullable: true),
                    was_reopened = table.Column<bool>(type: "boolean", nullable: false),
                    reopened_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    reopen_reason = table.Column<string>(type: "text", nullable: true),
                    matched_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    keyword_matches = table.Column<string[]>(type: "text[]", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("auto_resolution_log_pkey", x => x.id);
                    table.ForeignKey(
                        name: "auto_resolution_log_kb_article_id_fkey",
                        column: x => x.kb_article_id,
                        principalSchema: "kb",
                        principalTable: "kb_articles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "auto_resolution_log_ticket_id_fkey",
                        column: x => x.ticket_id,
                        principalSchema: "ticket",
                        principalTable: "tickets",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "Full audit of auto-resolution engine activity. One row per resolution attempt (successful or blocked). was_reopened=TRUE is the primary negative signal for the learning loop — the learning job reads this to raise confidence_threshold on kb_articles with high reopen rates. blocked_guardrail action means payroll/financial/legal guardrail fired.");

            migrationBuilder.CreateTable(
                name: "automation_execution_log",
                schema: "sla",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    rule_id = table.Column<Guid>(type: "uuid", nullable: false),
                    ticket_id = table.Column<Guid>(type: "uuid", nullable: false),
                    succeeded = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    error_message = table.Column<string>(type: "text", nullable: true),
                    executed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    RowVersion = table.Column<byte[]>(type: "bytea", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("automation_execution_log_pkey", x => x.id);
                    table.ForeignKey(
                        name: "automation_execution_log_rule_id_fkey",
                        column: x => x.rule_id,
                        principalSchema: "sla",
                        principalTable: "automation_rules",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "automation_execution_log_ticket_id_fkey",
                        column: x => x.ticket_id,
                        principalSchema: "ticket",
                        principalTable: "tickets",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "csat_surveys",
                schema: "ticket",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    ticket_id = table.Column<Guid>(type: "uuid", nullable: false),
                    contact_id = table.Column<Guid>(type: "uuid", nullable: true),
                    rating = table.Column<int>(type: "integer", nullable: false),
                    feedback = table.Column<string>(type: "text", nullable: true),
                    submitted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    row_version = table.Column<byte[]>(type: "bytea", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("csat_surveys_pkey", x => x.id);
                    table.ForeignKey(
                        name: "csat_surveys_contact_id_fkey",
                        column: x => x.contact_id,
                        principalSchema: "company",
                        principalTable: "contacts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "csat_surveys_ticket_id_fkey",
                        column: x => x.ticket_id,
                        principalSchema: "ticket",
                        principalTable: "tickets",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "notification_logs",
                schema: "notification",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    ticket_id = table.Column<Guid>(type: "uuid", nullable: true),
                    template_id = table.Column<Guid>(type: "uuid", nullable: false),
                    recipient_email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    error_message = table.Column<string>(type: "text", nullable: true),
                    is_failed_delivery = table.Column<bool>(type: "boolean", nullable: false),
                    sent_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("notification_logs_pkey", x => x.id);
                    table.ForeignKey(
                        name: "notification_logs_template_id_fkey",
                        column: x => x.template_id,
                        principalSchema: "notification",
                        principalTable: "notification_templates",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "notification_logs_ticket_id_fkey",
                        column: x => x.ticket_id,
                        principalSchema: "ticket",
                        principalTable: "tickets",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "sla_tickets",
                schema: "sla",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    ticket_id = table.Column<Guid>(type: "uuid", nullable: false),
                    policy_id = table.Column<Guid>(type: "uuid", nullable: false),
                    first_response_due_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    first_response_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    resolution_due_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    resolved_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    first_response_breached = table.Column<bool>(type: "boolean", nullable: false),
                    resolution_breached = table.Column<bool>(type: "boolean", nullable: false),
                    paused_minutes = table.Column<int>(type: "integer", nullable: false),
                    last_paused_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    follow_up_due_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, comment: "Third SLA stage: follow-up deadline. Typically set when ticket enters pending_customer status — agent must follow up if no customer response within N business hours. Prevents stale pending tickets."),
                    follow_up_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    follow_up_breached = table.Column<bool>(type: "boolean", nullable: false, comment: "TRUE when follow_up_at IS NULL AND NOW() > follow_up_due_at. Set by SLA engine daemon alongside first_response_breached and resolution_breached."),
                    RowVersion = table.Column<byte[]>(type: "bytea", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true, defaultValueSql: "now()"),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("sla_tickets_pkey", x => x.id);
                    table.ForeignKey(
                        name: "sla_tickets_policy_id_fkey",
                        column: x => x.policy_id,
                        principalSchema: "sla",
                        principalTable: "sla_policies",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "sla_tickets_ticket_id_fkey",
                        column: x => x.ticket_id,
                        principalSchema: "ticket",
                        principalTable: "tickets",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "Per-ticket SLA clock. One-to-one with tickets. paused_minutes accumulates total pause time from On Hold / Product Roadmap / Pending states. resolution_due_at is extended on resume. Breach flags set by SLA engine daemon / scheduled job.");

            migrationBuilder.CreateTable(
                name: "ticket_assignment_log",
                schema: "ticket",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    ticket_id = table.Column<Guid>(type: "uuid", nullable: false),
                    from_agent_id = table.Column<Guid>(type: "uuid", nullable: true),
                    to_agent_id = table.Column<Guid>(type: "uuid", nullable: false),
                    changed_by = table.Column<Guid>(type: "uuid", nullable: true),
                    assigned_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    row_version = table.Column<byte[]>(type: "bytea", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("ticket_assignment_log_pkey", x => x.id);
                    table.ForeignKey(
                        name: "ticket_assignment_log_changed_by_fkey",
                        column: x => x.changed_by,
                        principalSchema: "auth",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "ticket_assignment_log_from_agent_id_fkey",
                        column: x => x.from_agent_id,
                        principalSchema: "auth",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "ticket_assignment_log_ticket_id_fkey",
                        column: x => x.ticket_id,
                        principalSchema: "ticket",
                        principalTable: "tickets",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "ticket_assignment_log_to_agent_id_fkey",
                        column: x => x.to_agent_id,
                        principalSchema: "auth",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ticket_classification",
                schema: "ai",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    ticket_id = table.Column<Guid>(type: "uuid", nullable: false),
                    classifier_label = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    classifier_confidence = table.Column<decimal>(type: "numeric(5,4)", precision: 5, scale: 4, nullable: true),
                    classifier_auto_routed = table.Column<bool>(type: "boolean", nullable: false),
                    retrieval_top_k_labels = table.Column<string>(type: "jsonb", nullable: true),
                    retrieval_consensus_label = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    retrieval_discrepancy = table.Column<bool>(type: "boolean", nullable: false),
                    llm_invoked = table.Column<bool>(type: "boolean", nullable: false),
                    llm_final_label = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    llm_reasoning = table.Column<string>(type: "text", nullable: true),
                    final_label = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    final_confidence = table.Column<decimal>(type: "numeric(5,4)", precision: 5, scale: 4, nullable: true),
                    model_version = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    classified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    row_version = table.Column<byte[]>(type: "bytea", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("ticket_classification_pkey", x => x.id);
                    table.ForeignKey(
                        name: "ticket_classification_ticket_id_fkey",
                        column: x => x.ticket_id,
                        principalSchema: "ticket",
                        principalTable: "tickets",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "One row per ticket — full C-R-L (Classifier-Retrieval-LLM) pipeline audit. classifier_auto_routed=TRUE means phase 2+3 were skipped (confidence ≥ threshold). retrieval_discrepancy=TRUE triggered Phase 3 LLM. llm_invoked tracks LLM cost exposure. model_version enables A/B testing.");

            migrationBuilder.CreateTable(
                name: "ticket_comments",
                schema: "ticket",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    ticket_id = table.Column<Guid>(type: "uuid", nullable: false),
                    author_id = table.Column<Guid>(type: "uuid", nullable: true),
                    contact_id = table.Column<Guid>(type: "uuid", nullable: true),
                    body = table.Column<string>(type: "text", nullable: false),
                    visibility = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    row_version = table.Column<byte[]>(type: "bytea", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true, defaultValueSql: "now()"),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("ticket_comments_pkey", x => x.id);
                    table.ForeignKey(
                        name: "ticket_comments_author_id_fkey",
                        column: x => x.author_id,
                        principalSchema: "auth",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "ticket_comments_contact_id_fkey",
                        column: x => x.contact_id,
                        principalSchema: "company",
                        principalTable: "contacts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "ticket_comments_created_by_fkey",
                        column: x => x.created_by,
                        principalSchema: "auth",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "ticket_comments_ticket_id_fkey",
                        column: x => x.ticket_id,
                        principalSchema: "ticket",
                        principalTable: "tickets",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ticket_custom_fields",
                schema: "ticket",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    ticket_id = table.Column<Guid>(type: "uuid", nullable: false),
                    field_key = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    field_value = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    row_version = table.Column<byte[]>(type: "bytea", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("ticket_custom_fields_pkey", x => x.id);
                    table.ForeignKey(
                        name: "ticket_custom_fields_ticket_id_fkey",
                        column: x => x.ticket_id,
                        principalSchema: "ticket",
                        principalTable: "tickets",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ticket_relations",
                schema: "ticket",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ParentTicketId = table.Column<Guid>(type: "uuid", nullable: false),
                    ChildTicketId = table.Column<Guid>(type: "uuid", nullable: false),
                    RelationType = table.Column<string>(type: "text", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "bytea", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ticket_relations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ticket_relations_tickets_ChildTicketId",
                        column: x => x.ChildTicketId,
                        principalSchema: "ticket",
                        principalTable: "tickets",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ticket_relations_tickets_ParentTicketId",
                        column: x => x.ParentTicketId,
                        principalSchema: "ticket",
                        principalTable: "tickets",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ticket_risk_scores",
                schema: "ai",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    ticket_id = table.Column<Guid>(type: "uuid", nullable: false),
                    impact_score = table.Column<decimal>(type: "numeric(4,2)", precision: 4, scale: 2, nullable: false),
                    users_affected_score = table.Column<decimal>(type: "numeric(4,2)", precision: 4, scale: 2, nullable: false),
                    modules_affected_score = table.Column<decimal>(type: "numeric(4,2)", precision: 4, scale: 2, nullable: false),
                    urgency_score = table.Column<decimal>(type: "numeric(4,2)", precision: 4, scale: 2, nullable: false),
                    urgency_keywords = table.Column<List<string>>(type: "text[]", nullable: true),
                    sentiment_score = table.Column<decimal>(type: "numeric(4,2)", precision: 4, scale: 2, nullable: false),
                    sentiment_label = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    sla_severity_score = table.Column<decimal>(type: "numeric(4,2)", precision: 4, scale: 2, nullable: false),
                    type_weight = table.Column<decimal>(type: "numeric(4,2)", precision: 4, scale: 2, nullable: false),
                    tier_weight = table.Column<decimal>(type: "numeric(4,2)", precision: 4, scale: 2, nullable: false),
                    final_score = table.Column<decimal>(type: "numeric(4,2)", precision: 4, scale: 2, nullable: false),
                    assigned_priority = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    force_p1_triggered = table.Column<bool>(type: "boolean", nullable: false),
                    force_p1_reason = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    business_critical_bump = table.Column<bool>(type: "boolean", nullable: false),
                    pilot_project_flag = table.Column<bool>(type: "boolean", nullable: false),
                    competitor_threat_flag = table.Column<bool>(type: "boolean", nullable: false),
                    trigger_event = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    customer_reply_count_at = table.Column<int>(type: "integer", nullable: true),
                    computed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    row_version = table.Column<byte[]>(type: "bytea", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("ticket_risk_scores_pkey", x => x.id);
                    table.ForeignKey(
                        name: "ticket_risk_scores_ticket_id_fkey",
                        column: x => x.ticket_id,
                        principalSchema: "ticket",
                        principalTable: "tickets",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "Full audit of every priority score computation run. Multiple rows per ticket because the re-evaluation loop fires on every customer reply and SLA status change. trigger_event shows what caused the re-score. customer_reply_count_at enables auditing how many replies pushed priority up.");

            migrationBuilder.CreateTable(
                name: "ticket_status_history",
                schema: "ticket",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    ticket_id = table.Column<Guid>(type: "uuid", nullable: false),
                    from_status = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    to_status = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    changed_by = table.Column<Guid>(type: "uuid", nullable: true),
                    changed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    reason = table.Column<string>(type: "text", nullable: true),
                    row_version = table.Column<byte[]>(type: "bytea", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("ticket_status_history_pkey", x => x.id);
                    table.ForeignKey(
                        name: "ticket_status_history_changed_by_fkey",
                        column: x => x.changed_by,
                        principalSchema: "auth",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "ticket_status_history_ticket_id_fkey",
                        column: x => x.ticket_id,
                        principalSchema: "ticket",
                        principalTable: "tickets",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                },
                comment: "Immutable audit log of every status transition. Never update or delete rows. Append-only; enables full status trail and SLA clock reconstruction.");

            migrationBuilder.CreateTable(
                name: "ticket_watchers",
                schema: "ticket",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    ticket_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    added_by = table.Column<Guid>(type: "uuid", nullable: false),
                    added_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    row_version = table.Column<byte[]>(type: "bytea", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ticket_watchers", x => x.id);
                    table.ForeignKey(
                        name: "FK_ticket_watchers_tickets_ticket_id",
                        column: x => x.ticket_id,
                        principalSchema: "ticket",
                        principalTable: "tickets",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ticket_attachments",
                schema: "ticket",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    ticket_id = table.Column<Guid>(type: "uuid", nullable: false),
                    comment_id = table.Column<Guid>(type: "uuid", nullable: true),
                    file_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    file_url = table.Column<string>(type: "text", nullable: false),
                    file_size_bytes = table.Column<long>(type: "bigint", nullable: false),
                    mime_type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    row_version = table.Column<byte[]>(type: "bytea", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("ticket_attachments_pkey", x => x.id);
                    table.ForeignKey(
                        name: "ticket_attachments_comment_id_fkey",
                        column: x => x.comment_id,
                        principalSchema: "ticket",
                        principalTable: "ticket_comments",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "ticket_attachments_created_by_fkey",
                        column: x => x.created_by,
                        principalSchema: "auth",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "ticket_attachments_ticket_id_fkey",
                        column: x => x.ticket_id,
                        principalSchema: "ticket",
                        principalTable: "tickets",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "idx_agent_badges_agent",
                schema: "gamification",
                table: "agent_badges",
                columns: new[] { "agent_id", "earned_at" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "idx_agent_badges_unnotified",
                schema: "gamification",
                table: "agent_badges",
                column: "notified",
                filter: "(notified = false)");

            migrationBuilder.CreateIndex(
                name: "IX_agent_badges_badge_id",
                schema: "gamification",
                table: "agent_badges",
                column: "badge_id");

            migrationBuilder.CreateIndex(
                name: "IX_agent_badges_ticket_id",
                schema: "gamification",
                table: "agent_badges",
                column: "ticket_id");

            migrationBuilder.CreateIndex(
                name: "idx_agent_challenges_agent",
                schema: "gamification",
                table: "agent_challenges",
                columns: new[] { "agent_id", "is_completed" });

            migrationBuilder.CreateIndex(
                name: "idx_agent_challenges_challenge",
                schema: "gamification",
                table: "agent_challenges",
                columns: new[] { "challenge_id", "is_completed" });

            migrationBuilder.CreateIndex(
                name: "uq_agent_challenges",
                schema: "gamification",
                table: "agent_challenges",
                columns: new[] { "agent_id", "challenge_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_agent_points_agent",
                schema: "gamification",
                table: "agent_points",
                columns: new[] { "agent_id", "earned_at" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "idx_agent_points_rule_daily",
                schema: "gamification",
                table: "agent_points",
                columns: new[] { "agent_id", "rule_id", "earned_date" });

            migrationBuilder.CreateIndex(
                name: "idx_agent_points_ticket",
                schema: "gamification",
                table: "agent_points",
                column: "ticket_id",
                filter: "(ticket_id IS NOT NULL)");

            migrationBuilder.CreateIndex(
                name: "IX_agent_points_PointRuleId",
                schema: "gamification",
                table: "agent_points",
                column: "PointRuleId");

            migrationBuilder.CreateIndex(
                name: "IX_agent_points_rule_id",
                schema: "gamification",
                table: "agent_points",
                column: "rule_id");

            migrationBuilder.CreateIndex(
                name: "uq_agent_streaks_agent",
                schema: "gamification",
                table: "agent_streaks",
                column: "agent_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_ai_logs_agent",
                schema: "ai",
                table: "ai_suggestion_logs",
                columns: new[] { "agent_id", "created_at" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "idx_ai_logs_ticket",
                schema: "ai",
                table: "ai_suggestion_logs",
                column: "ticket_id");

            migrationBuilder.CreateIndex(
                name: "idx_audit_log_record",
                schema: "audit",
                table: "audit_log",
                columns: new[] { "table_name", "record_id", "changed_at" },
                descending: new[] { false, false, true });

            migrationBuilder.CreateIndex(
                name: "idx_audit_log_table",
                schema: "audit",
                table: "audit_log",
                columns: new[] { "table_name", "changed_at" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "idx_audit_log_user",
                schema: "audit",
                table: "audit_log",
                columns: new[] { "changed_by", "changed_at" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "idx_auto_res_action",
                schema: "ai",
                table: "auto_resolution_log",
                columns: new[] { "action_taken", "matched_at" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "idx_auto_res_article",
                schema: "ai",
                table: "auto_resolution_log",
                columns: new[] { "kb_article_id", "matched_at" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "idx_auto_res_reopened",
                schema: "ai",
                table: "auto_resolution_log",
                columns: new[] { "was_reopened", "matched_at" },
                descending: new[] { false, true },
                filter: "(was_reopened = true)");

            migrationBuilder.CreateIndex(
                name: "idx_auto_res_ticket",
                schema: "ai",
                table: "auto_resolution_log",
                columns: new[] { "ticket_id", "matched_at" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "idx_ael_failed",
                schema: "sla",
                table: "automation_execution_log",
                columns: new[] { "succeeded", "executed_at" },
                filter: "(succeeded = false)");

            migrationBuilder.CreateIndex(
                name: "idx_ael_rule",
                schema: "sla",
                table: "automation_execution_log",
                columns: new[] { "rule_id", "executed_at" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "idx_ael_ticket",
                schema: "sla",
                table: "automation_execution_log",
                columns: new[] { "ticket_id", "executed_at" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "idx_automation_rules_actions",
                schema: "sla",
                table: "automation_rules",
                column: "actions")
                .Annotation("Npgsql:IndexMethod", "gin");

            migrationBuilder.CreateIndex(
                name: "idx_automation_rules_conditions",
                schema: "sla",
                table: "automation_rules",
                column: "conditions")
                .Annotation("Npgsql:IndexMethod", "gin");

            migrationBuilder.CreateIndex(
                name: "IX_automation_rules_created_by",
                schema: "sla",
                table: "automation_rules",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "IX_automation_rules_updated_by",
                schema: "sla",
                table: "automation_rules",
                column: "updated_by");

            migrationBuilder.CreateIndex(
                name: "IX_badges_created_by",
                schema: "gamification",
                table: "badges",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "uq_badges_code",
                schema: "gamification",
                table: "badges",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_business_hours_region",
                schema: "sla",
                table: "business_hours",
                column: "geo_region",
                filter: "(is_deleted = false)");

            migrationBuilder.CreateIndex(
                name: "IX_business_hours_created_by",
                schema: "sla",
                table: "business_hours",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "idx_challenges_active",
                schema: "gamification",
                table: "challenges",
                columns: new[] { "is_active", "starts_at", "ends_at" });

            migrationBuilder.CreateIndex(
                name: "IX_challenges_badge_id",
                schema: "gamification",
                table: "challenges",
                column: "badge_id");

            migrationBuilder.CreateIndex(
                name: "IX_challenges_created_by",
                schema: "gamification",
                table: "challenges",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "IX_challenges_group_id",
                schema: "gamification",
                table: "challenges",
                column: "group_id");

            migrationBuilder.CreateIndex(
                name: "idx_companies_geo_region",
                schema: "company",
                table: "companies",
                column: "geo_region",
                filter: "(is_deleted = false)");

            migrationBuilder.CreateIndex(
                name: "idx_companies_health_score",
                schema: "company",
                table: "companies",
                column: "health_score",
                descending: new bool[0],
                filter: "(is_deleted = false)");

            migrationBuilder.CreateIndex(
                name: "idx_companies_name_trgm",
                schema: "company",
                table: "companies",
                column: "name",
                filter: "(is_deleted = false)")
                .Annotation("Npgsql:IndexMethod", "gin")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });

            migrationBuilder.CreateIndex(
                name: "idx_companies_payment_hold",
                schema: "company",
                table: "companies",
                column: "payment_on_hold",
                filter: "((payment_on_hold = true) AND (is_deleted = false))");

            migrationBuilder.CreateIndex(
                name: "idx_companies_tier",
                schema: "company",
                table: "companies",
                column: "support_tier",
                filter: "(is_deleted = false)");

            migrationBuilder.CreateIndex(
                name: "IX_companies_cam_user_id",
                schema: "company",
                table: "companies",
                column: "cam_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_companies_created_by",
                schema: "company",
                table: "companies",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "IX_companies_delivery_manager_id",
                schema: "company",
                table: "companies",
                column: "delivery_manager_id");

            migrationBuilder.CreateIndex(
                name: "IX_companies_updated_by",
                schema: "company",
                table: "companies",
                column: "updated_by");

            migrationBuilder.CreateIndex(
                name: "uq_companies_csp_id",
                schema: "company",
                table: "companies",
                column: "csp_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_company_contacts_limit_created_by",
                schema: "company",
                table: "company_contacts_limit",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "uq_company_contacts_limit_company",
                schema: "company",
                table: "company_contacts_limit",
                column: "company_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_company_domains_company",
                schema: "company",
                table: "company_domains",
                column: "company_id",
                filter: "(is_deleted = false)");

            migrationBuilder.CreateIndex(
                name: "idx_company_domains_domain",
                schema: "company",
                table: "company_domains",
                column: "domain");

            migrationBuilder.CreateIndex(
                name: "uq_company_domains_domain",
                schema: "company",
                table: "company_domains",
                column: "domain",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_contacts_authorized",
                schema: "company",
                table: "contacts",
                columns: new[] { "company_id", "is_authorized" },
                filter: "(is_deleted = false)");

            migrationBuilder.CreateIndex(
                name: "idx_contacts_company",
                schema: "company",
                table: "contacts",
                column: "company_id",
                filter: "(is_deleted = false)");

            migrationBuilder.CreateIndex(
                name: "IX_contacts_created_by",
                schema: "company",
                table: "contacts",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "IX_contacts_updated_by",
                schema: "company",
                table: "contacts",
                column: "updated_by");

            migrationBuilder.CreateIndex(
                name: "IX_contacts_user_id",
                schema: "company",
                table: "contacts",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "idx_csat_surveys_contact",
                schema: "ticket",
                table: "csat_surveys",
                column: "contact_id");

            migrationBuilder.CreateIndex(
                name: "idx_csat_surveys_rating",
                schema: "ticket",
                table: "csat_surveys",
                column: "rating");

            migrationBuilder.CreateIndex(
                name: "uq_csat_surveys_ticket",
                schema: "ticket",
                table: "csat_surveys",
                column: "ticket_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "uq_customer_status_map",
                schema: "config",
                table: "customer_status_map",
                column: "agent_status",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "uq_customer_tiers_code",
                schema: "lookup",
                table: "customer_tiers",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_escalation_rules_active",
                schema: "sla",
                table: "escalation_rules",
                columns: new[] { "is_active", "no_response_minutes" });

            migrationBuilder.CreateIndex(
                name: "IX_escalation_rules_created_by",
                schema: "sla",
                table: "escalation_rules",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "uq_geo_regions_code",
                schema: "lookup",
                table: "geo_regions",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_groups_region",
                schema: "auth",
                table: "groups",
                column: "region_code",
                filter: "(is_deleted = false)");

            migrationBuilder.CreateIndex(
                name: "idx_groups_tier",
                schema: "auth",
                table: "groups",
                column: "tier_code",
                filter: "(is_deleted = false)");

            migrationBuilder.CreateIndex(
                name: "IX_groups_created_by",
                schema: "auth",
                table: "groups",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "IX_groups_updated_by",
                schema: "auth",
                table: "groups",
                column: "updated_by");

            migrationBuilder.CreateIndex(
                name: "idx_holidays_region_date",
                schema: "sla",
                table: "holidays",
                columns: new[] { "geo_region", "holiday_date" },
                filter: "(is_deleted = false)");

            migrationBuilder.CreateIndex(
                name: "IX_holidays_created_by",
                schema: "sla",
                table: "holidays",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "uq_holidays",
                schema: "sla",
                table: "holidays",
                columns: new[] { "geo_region", "holiday_date" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_kb_articles_auto_resolve",
                schema: "kb",
                table: "kb_articles",
                columns: new[] { "auto_resolve", "guardrail_excluded" },
                filter: "((auto_resolve = true) AND (is_deleted = false))");

            migrationBuilder.CreateIndex(
                name: "idx_kb_articles_folder",
                schema: "kb",
                table: "kb_articles",
                column: "folder_id",
                filter: "(is_deleted = false)");

            migrationBuilder.CreateIndex(
                name: "idx_kb_articles_keywords",
                schema: "kb",
                table: "kb_articles",
                column: "keywords",
                filter: "((auto_resolve = true) AND (is_deleted = false))")
                .Annotation("Npgsql:IndexMethod", "gin");

            migrationBuilder.CreateIndex(
                name: "idx_kb_articles_published",
                schema: "kb",
                table: "kb_articles",
                columns: new[] { "is_published", "article_type" },
                filter: "(is_deleted = false)");

            migrationBuilder.CreateIndex(
                name: "idx_kb_articles_title_trgm",
                schema: "kb",
                table: "kb_articles",
                column: "title",
                filter: "(is_deleted = false)")
                .Annotation("Npgsql:IndexMethod", "gin")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });

            migrationBuilder.CreateIndex(
                name: "IX_kb_articles_author_id",
                schema: "kb",
                table: "kb_articles",
                column: "author_id");

            migrationBuilder.CreateIndex(
                name: "IX_kb_articles_created_by",
                schema: "kb",
                table: "kb_articles",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "IX_kb_articles_updated_by",
                schema: "kb",
                table: "kb_articles",
                column: "updated_by");

            migrationBuilder.CreateIndex(
                name: "idx_kb_attachments_article",
                schema: "kb",
                table: "kb_attachments",
                column: "article_id",
                filter: "(is_deleted = false)");

            migrationBuilder.CreateIndex(
                name: "idx_kb_folders_parent",
                schema: "kb",
                table: "kb_folders",
                column: "parent_id",
                filter: "(is_deleted = false)");

            migrationBuilder.CreateIndex(
                name: "IX_kb_folders_created_by",
                schema: "kb",
                table: "kb_folders",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "IX_kb_folders_updated_by",
                schema: "kb",
                table: "kb_folders",
                column: "updated_by");

            migrationBuilder.CreateIndex(
                name: "IX_leaderboard_snapshots_agent_id",
                schema: "gamification",
                table: "leaderboard_snapshots",
                column: "agent_id");

            migrationBuilder.CreateIndex(
                name: "IX_leaderboard_snapshots_group_id",
                schema: "gamification",
                table: "leaderboard_snapshots",
                column: "group_id");

            migrationBuilder.CreateIndex(
                name: "idx_modules_active",
                schema: "lookup",
                table: "modules",
                column: "is_active",
                filter: "(is_deleted = false)");

            migrationBuilder.CreateIndex(
                name: "idx_modules_department",
                schema: "lookup",
                table: "modules",
                column: "department",
                filter: "(is_deleted = false)");

            migrationBuilder.CreateIndex(
                name: "IX_modules_created_by",
                schema: "lookup",
                table: "modules",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "IX_modules_updated_by",
                schema: "lookup",
                table: "modules",
                column: "updated_by");

            migrationBuilder.CreateIndex(
                name: "uq_modules_code",
                schema: "lookup",
                table: "modules",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_notification_logs_failed",
                schema: "notification",
                table: "notification_logs",
                columns: new[] { "is_failed_delivery", "sent_at" },
                filter: "(is_failed_delivery = true)");

            migrationBuilder.CreateIndex(
                name: "idx_notification_logs_template",
                schema: "notification",
                table: "notification_logs",
                column: "template_id");

            migrationBuilder.CreateIndex(
                name: "idx_notification_logs_ticket",
                schema: "notification",
                table: "notification_logs",
                columns: new[] { "ticket_id", "sent_at" },
                descending: new[] { false, true },
                filter: "(ticket_id IS NOT NULL)");

            migrationBuilder.CreateIndex(
                name: "IX_notification_templates_created_by",
                schema: "notification",
                table: "notification_templates",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "IX_notification_templates_updated_by",
                schema: "notification",
                table: "notification_templates",
                column: "updated_by");

            migrationBuilder.CreateIndex(
                name: "uq_notification_templates_code",
                schema: "notification",
                table: "notification_templates",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_permissions_created_by",
                schema: "auth",
                table: "permissions",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "IX_permissions_updated_by",
                schema: "auth",
                table: "permissions",
                column: "updated_by");

            migrationBuilder.CreateIndex(
                name: "uq_permissions_resource_action",
                schema: "auth",
                table: "permissions",
                columns: new[] { "resource", "action" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_point_rules_created_by",
                schema: "gamification",
                table: "point_rules",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "uq_point_rules_code",
                schema: "gamification",
                table: "point_rules",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_portal_banners_active",
                schema: "kb",
                table: "portal_banners",
                columns: new[] { "is_active", "active_from", "active_to" },
                filter: "(is_active = true)");

            migrationBuilder.CreateIndex(
                name: "IX_portal_banners_created_by",
                schema: "kb",
                table: "portal_banners",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "IX_portal_banners_updated_by",
                schema: "kb",
                table: "portal_banners",
                column: "updated_by");

            migrationBuilder.CreateIndex(
                name: "idx_product_versions_active",
                schema: "lookup",
                table: "product_versions",
                column: "is_active",
                filter: "(is_deleted = false)");

            migrationBuilder.CreateIndex(
                name: "IX_product_versions_created_by",
                schema: "lookup",
                table: "product_versions",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "IX_product_versions_updated_by",
                schema: "lookup",
                table: "product_versions",
                column: "updated_by");

            migrationBuilder.CreateIndex(
                name: "uq_product_versions_code",
                schema: "lookup",
                table: "product_versions",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_refresh_tokens_expires",
                schema: "auth",
                table: "refresh_tokens",
                column: "expires_at",
                filter: "(is_revoked = false)");

            migrationBuilder.CreateIndex(
                name: "idx_refresh_tokens_family",
                schema: "auth",
                table: "refresh_tokens",
                column: "family_id");

            migrationBuilder.CreateIndex(
                name: "idx_refresh_tokens_user",
                schema: "auth",
                table: "refresh_tokens",
                column: "user_id",
                filter: "(is_revoked = false)");

            migrationBuilder.CreateIndex(
                name: "IX_refresh_tokens_replaced_by_token_id",
                schema: "auth",
                table: "refresh_tokens",
                column: "replaced_by_token_id");

            migrationBuilder.CreateIndex(
                name: "uq_refresh_tokens_hash",
                schema: "auth",
                table: "refresh_tokens",
                column: "token_hash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_role_permissions_role",
                schema: "auth",
                table: "role_permissions",
                column: "role_id");

            migrationBuilder.CreateIndex(
                name: "IX_role_permissions_created_by",
                schema: "auth",
                table: "role_permissions",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "IX_role_permissions_permission_id",
                schema: "auth",
                table: "role_permissions",
                column: "permission_id");

            migrationBuilder.CreateIndex(
                name: "IX_role_permissions_updated_by",
                schema: "auth",
                table: "role_permissions",
                column: "updated_by");

            migrationBuilder.CreateIndex(
                name: "uq_role_permissions",
                schema: "auth",
                table: "role_permissions",
                columns: new[] { "role_id", "permission_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_roles_created_by",
                schema: "auth",
                table: "roles",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "IX_roles_updated_by",
                schema: "auth",
                table: "roles",
                column: "updated_by");

            migrationBuilder.CreateIndex(
                name: "uq_roles_name_exact",
                schema: "auth",
                table: "roles",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_sla_policies_lookup",
                schema: "sla",
                table: "sla_policies",
                columns: new[] { "geo_region", "tier_code", "priority" },
                filter: "(is_active = true)");

            migrationBuilder.CreateIndex(
                name: "IX_sla_policies_created_by",
                schema: "sla",
                table: "sla_policies",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "IX_sla_policies_tier_code",
                schema: "sla",
                table: "sla_policies",
                column: "tier_code");

            migrationBuilder.CreateIndex(
                name: "IX_sla_policies_updated_by",
                schema: "sla",
                table: "sla_policies",
                column: "updated_by");

            migrationBuilder.CreateIndex(
                name: "uq_sla_policies",
                schema: "sla",
                table: "sla_policies",
                columns: new[] { "geo_region", "tier_code", "priority" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_sla_follow_up_due",
                schema: "sla",
                table: "sla_tickets",
                column: "follow_up_due_at",
                filter: "((follow_up_at IS NULL) AND (follow_up_due_at IS NOT NULL))");

            migrationBuilder.CreateIndex(
                name: "idx_sla_tickets_breached",
                schema: "sla",
                table: "sla_tickets",
                columns: new[] { "first_response_breached", "resolution_breached" });

            migrationBuilder.CreateIndex(
                name: "idx_sla_tickets_due",
                schema: "sla",
                table: "sla_tickets",
                column: "resolution_due_at",
                filter: "(resolved_at IS NULL)");

            migrationBuilder.CreateIndex(
                name: "IX_sla_tickets_policy_id",
                schema: "sla",
                table: "sla_tickets",
                column: "policy_id");

            migrationBuilder.CreateIndex(
                name: "uq_sla_tickets_ticket",
                schema: "sla",
                table: "sla_tickets",
                column: "ticket_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "uq_solution_types_code",
                schema: "lookup",
                table: "solution_types",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_status_transitions_from",
                schema: "workflow",
                table: "status_transitions",
                columns: new[] { "graph_id", "from_status" },
                filter: "(is_deleted = false)");

            migrationBuilder.CreateIndex(
                name: "idx_status_transitions_graph",
                schema: "workflow",
                table: "status_transitions",
                column: "graph_id",
                filter: "(is_deleted = false)");

            migrationBuilder.CreateIndex(
                name: "IX_status_transitions_created_by",
                schema: "workflow",
                table: "status_transitions",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "IX_status_transitions_requires_role",
                schema: "workflow",
                table: "status_transitions",
                column: "requires_role");

            migrationBuilder.CreateIndex(
                name: "uq_status_transitions",
                schema: "workflow",
                table: "status_transitions",
                columns: new[] { "graph_id", "from_status", "to_status" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_sub_modules_module_active",
                schema: "lookup",
                table: "sub_modules",
                columns: new[] { "module_id", "is_active" },
                filter: "(is_deleted = false)");

            migrationBuilder.CreateIndex(
                name: "idx_sub_modules_module_id",
                schema: "lookup",
                table: "sub_modules",
                column: "module_id");

            migrationBuilder.CreateIndex(
                name: "IX_sub_modules_created_by",
                schema: "lookup",
                table: "sub_modules",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "IX_sub_modules_updated_by",
                schema: "lookup",
                table: "sub_modules",
                column: "updated_by");

            migrationBuilder.CreateIndex(
                name: "uq_sub_modules_code",
                schema: "lookup",
                table: "sub_modules",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_tal_agent",
                schema: "ticket",
                table: "ticket_assignment_log",
                columns: new[] { "to_agent_id", "assigned_at" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "idx_tal_ticket",
                schema: "ticket",
                table: "ticket_assignment_log",
                columns: new[] { "ticket_id", "assigned_at" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "IX_ticket_assignment_log_changed_by",
                schema: "ticket",
                table: "ticket_assignment_log",
                column: "changed_by");

            migrationBuilder.CreateIndex(
                name: "IX_ticket_assignment_log_from_agent_id",
                schema: "ticket",
                table: "ticket_assignment_log",
                column: "from_agent_id");

            migrationBuilder.CreateIndex(
                name: "IX_ticket_assignment_log_ticket_id_from_agent_id_to_agent_id",
                schema: "ticket",
                table: "ticket_assignment_log",
                columns: new[] { "ticket_id", "from_agent_id", "to_agent_id" });

            migrationBuilder.CreateIndex(
                name: "idx_ticket_attachments_comment",
                schema: "ticket",
                table: "ticket_attachments",
                column: "comment_id",
                filter: "((comment_id IS NOT NULL) AND (is_deleted = false))");

            migrationBuilder.CreateIndex(
                name: "idx_ticket_attachments_ticket",
                schema: "ticket",
                table: "ticket_attachments",
                column: "ticket_id",
                filter: "(is_deleted = false)");

            migrationBuilder.CreateIndex(
                name: "IX_ticket_attachments_created_by",
                schema: "ticket",
                table: "ticket_attachments",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "idx_ticket_classification_label",
                schema: "ai",
                table: "ticket_classification",
                columns: new[] { "final_label", "classified_at" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "idx_ticket_classification_llm",
                schema: "ai",
                table: "ticket_classification",
                columns: new[] { "llm_invoked", "classified_at" },
                descending: new[] { false, true },
                filter: "(llm_invoked = true)");

            migrationBuilder.CreateIndex(
                name: "idx_ticket_classification_ticket",
                schema: "ai",
                table: "ticket_classification",
                column: "ticket_id");

            migrationBuilder.CreateIndex(
                name: "uq_ticket_classification_ticket",
                schema: "ai",
                table: "ticket_classification",
                column: "ticket_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_ticket_comments_author",
                schema: "ticket",
                table: "ticket_comments",
                column: "author_id",
                filter: "(is_deleted = false)");

            migrationBuilder.CreateIndex(
                name: "idx_ticket_comments_ticket",
                schema: "ticket",
                table: "ticket_comments",
                columns: new[] { "ticket_id", "created_at" },
                filter: "(is_deleted = false)");

            migrationBuilder.CreateIndex(
                name: "IX_ticket_comments_contact_id",
                schema: "ticket",
                table: "ticket_comments",
                column: "contact_id");

            migrationBuilder.CreateIndex(
                name: "IX_ticket_comments_created_by",
                schema: "ticket",
                table: "ticket_comments",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "idx_tcf_key",
                schema: "ticket",
                table: "ticket_custom_fields",
                columns: new[] { "field_key", "field_value" });

            migrationBuilder.CreateIndex(
                name: "idx_tcf_ticket",
                schema: "ticket",
                table: "ticket_custom_fields",
                column: "ticket_id");

            migrationBuilder.CreateIndex(
                name: "IX_ticket_relations_ChildTicketId",
                schema: "ticket",
                table: "ticket_relations",
                column: "ChildTicketId");

            migrationBuilder.CreateIndex(
                name: "IX_ticket_relations_ParentTicketId_ChildTicketId_RelationType",
                schema: "ticket",
                table: "ticket_relations",
                columns: new[] { "ParentTicketId", "ChildTicketId", "RelationType" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_risk_scores_force_p1",
                schema: "ai",
                table: "ticket_risk_scores",
                columns: new[] { "force_p1_triggered", "computed_at" },
                descending: new[] { false, true },
                filter: "(force_p1_triggered = true)");

            migrationBuilder.CreateIndex(
                name: "idx_risk_scores_high_priority",
                schema: "ai",
                table: "ticket_risk_scores",
                columns: new[] { "final_score", "computed_at" },
                descending: new bool[0],
                filter: "(final_score >= 3.5)");

            migrationBuilder.CreateIndex(
                name: "idx_risk_scores_ticket",
                schema: "ai",
                table: "ticket_risk_scores",
                columns: new[] { "ticket_id", "computed_at" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "idx_tsgs_graph",
                schema: "workflow",
                table: "ticket_status_graph_scopes",
                column: "graph_id",
                filter: "(is_deleted = false)");

            migrationBuilder.CreateIndex(
                name: "idx_tsgs_resolution",
                schema: "workflow",
                table: "ticket_status_graph_scopes",
                columns: new[] { "priority", "version_id", "module_id", "sub_module_id" },
                descending: new[] { true, false, false, false },
                filter: "((is_active = true) AND (is_deleted = false))");

            migrationBuilder.CreateIndex(
                name: "IX_ticket_status_graph_scopes_created_by",
                schema: "workflow",
                table: "ticket_status_graph_scopes",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "IX_ticket_status_graph_scopes_module_id",
                schema: "workflow",
                table: "ticket_status_graph_scopes",
                column: "module_id");

            migrationBuilder.CreateIndex(
                name: "IX_ticket_status_graph_scopes_sub_module_id",
                schema: "workflow",
                table: "ticket_status_graph_scopes",
                column: "sub_module_id");

            migrationBuilder.CreateIndex(
                name: "IX_ticket_status_graph_scopes_updated_by",
                schema: "workflow",
                table: "ticket_status_graph_scopes",
                column: "updated_by");

            migrationBuilder.CreateIndex(
                name: "IX_ticket_status_graph_scopes_version_id",
                schema: "workflow",
                table: "ticket_status_graph_scopes",
                column: "version_id");

            migrationBuilder.CreateIndex(
                name: "idx_tsg_active",
                schema: "workflow",
                table: "ticket_status_graphs",
                column: "is_active",
                filter: "(is_deleted = false)");

            migrationBuilder.CreateIndex(
                name: "IX_ticket_status_graphs_created_by",
                schema: "workflow",
                table: "ticket_status_graphs",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "IX_ticket_status_graphs_updated_by",
                schema: "workflow",
                table: "ticket_status_graphs",
                column: "updated_by");

            migrationBuilder.CreateIndex(
                name: "uq_ticket_status_graphs_code",
                schema: "workflow",
                table: "ticket_status_graphs",
                column: "graph_code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_tsh_ticket_time",
                schema: "ticket",
                table: "ticket_status_history",
                columns: new[] { "ticket_id", "changed_at" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "IX_ticket_status_history_changed_by",
                schema: "ticket",
                table: "ticket_status_history",
                column: "changed_by");

            migrationBuilder.CreateIndex(
                name: "IX_ticket_watchers_ticket_id_user_id",
                schema: "ticket",
                table: "ticket_watchers",
                columns: new[] { "ticket_id", "user_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_tickets_assigned_agent_id",
                schema: "ticket",
                table: "tickets",
                column: "assigned_agent_id");

            migrationBuilder.CreateIndex(
                name: "idx_tickets_auto_resolved",
                schema: "ticket",
                table: "tickets",
                columns: new[] { "is_auto_resolved", "created_at" },
                filter: "((is_auto_resolved = true) AND (is_deleted = false))");

            migrationBuilder.CreateIndex(
                name: "idx_tickets_company_id",
                schema: "ticket",
                table: "tickets",
                column: "company_id");

            migrationBuilder.CreateIndex(
                name: "idx_tickets_created_at",
                schema: "ticket",
                table: "tickets",
                column: "created_at",
                descending: new bool[0],
                filter: "(is_deleted = false)");

            migrationBuilder.CreateIndex(
                name: "idx_tickets_force_p1",
                schema: "ticket",
                table: "tickets",
                column: "force_p1",
                filter: "((force_p1 = true) AND (is_deleted = false))");

            migrationBuilder.CreateIndex(
                name: "idx_tickets_payment_hold",
                schema: "ticket",
                table: "tickets",
                column: "is_on_hold_payment",
                filter: "((is_on_hold_payment = true) AND (is_deleted = false))");

            migrationBuilder.CreateIndex(
                name: "idx_tickets_priority_score",
                schema: "ticket",
                table: "tickets",
                column: "priority_score",
                descending: new bool[0],
                filter: "(is_deleted = false)")
                .Annotation("Npgsql:IndexNullSortOrder", new[] { NullSortOrder.NullsLast });

            migrationBuilder.CreateIndex(
                name: "idx_tickets_solution_type",
                schema: "ticket",
                table: "tickets",
                column: "solution_type_id",
                filter: "((solution_type_id IS NOT NULL) AND (is_deleted = false))");

            migrationBuilder.CreateIndex(
                name: "idx_tickets_status",
                schema: "ticket",
                table: "tickets",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "idx_tickets_ticket_number",
                schema: "ticket",
                table: "tickets",
                column: "ticket_number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_tickets_version",
                schema: "ticket",
                table: "tickets",
                column: "version_id",
                filter: "(is_deleted = false)");

            migrationBuilder.CreateIndex(
                name: "IX_tickets_audited_by",
                schema: "ticket",
                table: "tickets",
                column: "audited_by");

            migrationBuilder.CreateIndex(
                name: "IX_tickets_contact_id",
                schema: "ticket",
                table: "tickets",
                column: "contact_id");

            migrationBuilder.CreateIndex(
                name: "IX_tickets_created_by",
                schema: "ticket",
                table: "tickets",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "IX_tickets_created_by_user_id",
                schema: "ticket",
                table: "tickets",
                column: "created_by_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_tickets_graph_id",
                schema: "ticket",
                table: "tickets",
                column: "graph_id");

            migrationBuilder.CreateIndex(
                name: "IX_tickets_group_id",
                schema: "ticket",
                table: "tickets",
                column: "group_id");

            migrationBuilder.CreateIndex(
                name: "IX_tickets_module_id",
                schema: "ticket",
                table: "tickets",
                column: "module_id");

            migrationBuilder.CreateIndex(
                name: "IX_tickets_sub_module_id",
                schema: "ticket",
                table: "tickets",
                column: "sub_module_id");

            migrationBuilder.CreateIndex(
                name: "IX_tickets_updated_by",
                schema: "ticket",
                table: "tickets",
                column: "updated_by");

            migrationBuilder.CreateIndex(
                name: "uq_tickets_number",
                schema: "ticket",
                table: "tickets",
                column: "ticket_number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_token_blacklist_expires",
                schema: "auth",
                table: "token_blacklist",
                column: "expires_at");

            migrationBuilder.CreateIndex(
                name: "idx_token_blacklist_user",
                schema: "auth",
                table: "token_blacklist",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "uq_token_blacklist_jti",
                schema: "auth",
                table: "token_blacklist",
                column: "jti",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_user_groups_group",
                schema: "auth",
                table: "user_groups",
                column: "group_id",
                filter: "(is_deleted = false)");

            migrationBuilder.CreateIndex(
                name: "idx_user_groups_lead",
                schema: "auth",
                table: "user_groups",
                columns: new[] { "group_id", "is_lead" },
                filter: "((is_lead = true) AND (is_deleted = false))");

            migrationBuilder.CreateIndex(
                name: "idx_user_groups_user",
                schema: "auth",
                table: "user_groups",
                column: "user_id",
                filter: "(is_deleted = false)");

            migrationBuilder.CreateIndex(
                name: "IX_user_groups_created_by",
                schema: "auth",
                table: "user_groups",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "uq_user_groups",
                schema: "auth",
                table: "user_groups",
                columns: new[] { "user_id", "group_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_otp_expires",
                schema: "auth",
                table: "user_otp_codes",
                column: "expires_at",
                filter: "(is_used = false)");

            migrationBuilder.CreateIndex(
                name: "idx_otp_user_purpose",
                schema: "auth",
                table: "user_otp_codes",
                columns: new[] { "user_id", "purpose" },
                filter: "(is_used = false)");

            migrationBuilder.CreateIndex(
                name: "idx_user_roles_role",
                schema: "auth",
                table: "user_roles",
                column: "role_id",
                filter: "(is_deleted = false)");

            migrationBuilder.CreateIndex(
                name: "idx_user_roles_user",
                schema: "auth",
                table: "user_roles",
                column: "user_id",
                filter: "(is_deleted = false)");

            migrationBuilder.CreateIndex(
                name: "IX_user_roles_assigned_by",
                schema: "auth",
                table: "user_roles",
                column: "assigned_by");

            migrationBuilder.CreateIndex(
                name: "IX_user_roles_created_by",
                schema: "auth",
                table: "user_roles",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "IX_user_roles_updated_by",
                schema: "auth",
                table: "user_roles",
                column: "updated_by");

            migrationBuilder.CreateIndex(
                name: "uq_user_roles",
                schema: "auth",
                table: "user_roles",
                columns: new[] { "user_id", "role_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_user_sessions_token",
                schema: "auth",
                table: "user_sessions",
                column: "refresh_token_id");

            migrationBuilder.CreateIndex(
                name: "idx_user_sessions_user",
                schema: "auth",
                table: "user_sessions",
                column: "user_id",
                filter: "(is_active = true)");

            migrationBuilder.CreateIndex(
                name: "idx_vtoken_expires",
                schema: "auth",
                table: "user_verification_tokens",
                column: "expires_at",
                filter: "(is_used = false)");

            migrationBuilder.CreateIndex(
                name: "idx_vtoken_user_purpose",
                schema: "auth",
                table: "user_verification_tokens",
                columns: new[] { "user_id", "purpose" },
                filter: "(is_used = false)");

            migrationBuilder.CreateIndex(
                name: "uq_verification_token_hash",
                schema: "auth",
                table: "user_verification_tokens",
                column: "token_hash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_users_active",
                schema: "auth",
                table: "users",
                column: "is_active",
                filter: "(is_deleted = false)");

            migrationBuilder.CreateIndex(
                name: "idx_users_last_login",
                schema: "auth",
                table: "users",
                column: "last_login_at",
                descending: new bool[0],
                filter: "(is_deleted = false)");

            migrationBuilder.CreateIndex(
                name: "idx_users_lockout",
                schema: "auth",
                table: "users",
                column: "lockout_end",
                filter: "((lockout_end IS NOT NULL) AND (is_deleted = false))");

            migrationBuilder.CreateIndex(
                name: "idx_users_username",
                schema: "auth",
                table: "users",
                column: "normalized_username",
                filter: "((is_deleted = false) AND (normalized_username IS NOT NULL))");

            migrationBuilder.CreateIndex(
                name: "IX_users_created_by",
                schema: "auth",
                table: "users",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "IX_users_updated_by",
                schema: "auth",
                table: "users",
                column: "updated_by");

            migrationBuilder.CreateIndex(
                name: "uq_users_normalized_email",
                schema: "auth",
                table: "users",
                column: "normalized_email",
                unique: true,
                filter: "(is_deleted = false)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "agent_badges",
                schema: "gamification");

            migrationBuilder.DropTable(
                name: "agent_challenges",
                schema: "gamification");

            migrationBuilder.DropTable(
                name: "agent_points",
                schema: "gamification");

            migrationBuilder.DropTable(
                name: "agent_streaks",
                schema: "gamification");

            migrationBuilder.DropTable(
                name: "ai_suggestion_logs",
                schema: "ai");

            migrationBuilder.DropTable(
                name: "audit_log",
                schema: "audit");

            migrationBuilder.DropTable(
                name: "auto_resolution_log",
                schema: "ai");

            migrationBuilder.DropTable(
                name: "automation_execution_log",
                schema: "sla");

            migrationBuilder.DropTable(
                name: "business_hours",
                schema: "sla");

            migrationBuilder.DropTable(
                name: "company_contacts_limit",
                schema: "company");

            migrationBuilder.DropTable(
                name: "company_domains",
                schema: "company");

            migrationBuilder.DropTable(
                name: "csat_surveys",
                schema: "ticket");

            migrationBuilder.DropTable(
                name: "customer_status_map",
                schema: "config");

            migrationBuilder.DropTable(
                name: "escalation_rules",
                schema: "sla");

            migrationBuilder.DropTable(
                name: "holidays",
                schema: "sla");

            migrationBuilder.DropTable(
                name: "kb_attachments",
                schema: "kb");

            migrationBuilder.DropTable(
                name: "leaderboard_snapshots",
                schema: "gamification");

            migrationBuilder.DropTable(
                name: "notification_logs",
                schema: "notification");

            migrationBuilder.DropTable(
                name: "portal_banners",
                schema: "kb");

            migrationBuilder.DropTable(
                name: "role_permissions",
                schema: "auth");

            migrationBuilder.DropTable(
                name: "sla_tickets",
                schema: "sla");

            migrationBuilder.DropTable(
                name: "status_transitions",
                schema: "workflow");

            migrationBuilder.DropTable(
                name: "ticket_assignment_log",
                schema: "ticket");

            migrationBuilder.DropTable(
                name: "ticket_attachments",
                schema: "ticket");

            migrationBuilder.DropTable(
                name: "ticket_classification",
                schema: "ai");

            migrationBuilder.DropTable(
                name: "ticket_custom_fields",
                schema: "ticket");

            migrationBuilder.DropTable(
                name: "ticket_relations",
                schema: "ticket");

            migrationBuilder.DropTable(
                name: "ticket_risk_scores",
                schema: "ai");

            migrationBuilder.DropTable(
                name: "ticket_status_graph_scopes",
                schema: "workflow");

            migrationBuilder.DropTable(
                name: "ticket_status_history",
                schema: "ticket");

            migrationBuilder.DropTable(
                name: "ticket_watchers",
                schema: "ticket");

            migrationBuilder.DropTable(
                name: "token_blacklist",
                schema: "auth");

            migrationBuilder.DropTable(
                name: "user_groups",
                schema: "auth");

            migrationBuilder.DropTable(
                name: "user_otp_codes",
                schema: "auth");

            migrationBuilder.DropTable(
                name: "user_roles",
                schema: "auth");

            migrationBuilder.DropTable(
                name: "user_sessions",
                schema: "auth");

            migrationBuilder.DropTable(
                name: "user_verification_tokens",
                schema: "auth");

            migrationBuilder.DropTable(
                name: "challenges",
                schema: "gamification");

            migrationBuilder.DropTable(
                name: "point_rules",
                schema: "gamification");

            migrationBuilder.DropTable(
                name: "automation_rules",
                schema: "sla");

            migrationBuilder.DropTable(
                name: "kb_articles",
                schema: "kb");

            migrationBuilder.DropTable(
                name: "notification_templates",
                schema: "notification");

            migrationBuilder.DropTable(
                name: "permissions",
                schema: "auth");

            migrationBuilder.DropTable(
                name: "sla_policies",
                schema: "sla");

            migrationBuilder.DropTable(
                name: "ticket_comments",
                schema: "ticket");

            migrationBuilder.DropTable(
                name: "roles",
                schema: "auth");

            migrationBuilder.DropTable(
                name: "refresh_tokens",
                schema: "auth");

            migrationBuilder.DropTable(
                name: "badges",
                schema: "gamification");

            migrationBuilder.DropTable(
                name: "kb_folders",
                schema: "kb");

            migrationBuilder.DropTable(
                name: "tickets",
                schema: "ticket");

            migrationBuilder.DropTable(
                name: "contacts",
                schema: "company");

            migrationBuilder.DropTable(
                name: "ticket_status_graphs",
                schema: "workflow");

            migrationBuilder.DropTable(
                name: "groups",
                schema: "auth");

            migrationBuilder.DropTable(
                name: "solution_types",
                schema: "lookup");

            migrationBuilder.DropTable(
                name: "sub_modules",
                schema: "lookup");

            migrationBuilder.DropTable(
                name: "product_versions",
                schema: "lookup");

            migrationBuilder.DropTable(
                name: "companies",
                schema: "company");

            migrationBuilder.DropTable(
                name: "modules",
                schema: "lookup");

            migrationBuilder.DropTable(
                name: "geo_regions",
                schema: "lookup");

            migrationBuilder.DropTable(
                name: "customer_tiers",
                schema: "lookup");

            migrationBuilder.DropTable(
                name: "users",
                schema: "auth");

            migrationBuilder.DropSequence(
                name: "ticket_number_seq",
                schema: "ticket");
        }
    }
}
