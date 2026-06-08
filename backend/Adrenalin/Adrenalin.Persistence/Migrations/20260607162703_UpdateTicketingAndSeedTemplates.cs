using System;
using System.Collections.Generic;
using System.Net;
using Adrenalin.Modules.Ticketing.Domain.Enums;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Adrenalin.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UpdateTicketingAndSeedTemplates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Rename subject to title
            migrationBuilder.DropColumn(
                name: "subject",
                schema: "ticket",
                table: "tickets");

            // Raw SQL to add enum values
            migrationBuilder.Sql("ALTER TYPE ticket.ticket_status ADD VALUE IF NOT EXISTS 'assigned';");
            migrationBuilder.Sql("ALTER TYPE ticket.ticket_status ADD VALUE IF NOT EXISTS 'pending';");

            migrationBuilder.AlterTable(
                name: "tickets",
                schema: "ticket",
                comment: "Central transactional entity.");

            // Alter status column from enum to varchar
            migrationBuilder.AlterColumn<string>(
                name: "status",
                schema: "ticket",
                table: "tickets",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                oldClrType: typeof(TicketStatus),
                oldType: "ticket.ticket_status");

            // Add the new columns
            migrationBuilder.AddColumn<Guid>(
                name: "AssigneeId",
                schema: "ticket",
                table: "tickets",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ReporterId",
                schema: "ticket",
                table: "tickets",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "category",
                schema: "ticket",
                table: "tickets",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "closed_at",
                schema: "ticket",
                table: "tickets",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "department",
                schema: "ticket",
                table: "tickets",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "module_name",
                schema: "ticket",
                table: "tickets",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "priority",
                schema: "ticket",
                table: "tickets",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "region",
                schema: "ticket",
                table: "tickets",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "resolved_at",
                schema: "ticket",
                table: "tickets",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "title",
                schema: "ticket",
                table: "tickets",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<List<string>>(
                name: "MentionedUsers",
                schema: "ticket",
                table: "ticket_comments",
                type: "text[]",
                nullable: false);

            // Create new ticketing tables
            migrationBuilder.CreateTable(
                name: "ticket_activities",
                schema: "ticket",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    ticket_id = table.Column<Guid>(type: "uuid", nullable: false),
                    activity_type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    old_value = table.Column<string>(type: "text", nullable: true),
                    new_value = table.Column<string>(type: "text", nullable: true),
                    performed_by = table.Column<Guid>(type: "uuid", nullable: true),
                    performed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "bytea", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("ticket_activities_pkey", x => x.id);
                    table.ForeignKey(
                        name: "ticket_activities_ticket_id_fkey",
                        column: x => x.ticket_id,
                        principalSchema: "ticket",
                        principalTable: "tickets",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ticket_tags",
                schema: "ticket",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    ticket_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tag_name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    RowVersion = table.Column<byte[]>(type: "bytea", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("ticket_tags_pkey", x => x.id);
                    table.ForeignKey(
                        name: "ticket_tags_ticket_id_fkey",
                        column: x => x.ticket_id,
                        principalSchema: "ticket",
                        principalTable: "tickets",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ticket_activities_ticket_id",
                schema: "ticket",
                table: "ticket_activities",
                column: "ticket_id");

            migrationBuilder.CreateIndex(
                name: "IX_ticket_tags_ticket_id",
                schema: "ticket",
                table: "ticket_tags",
                column: "ticket_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ticket_activities",
                schema: "ticket");

            migrationBuilder.DropTable(
                name: "ticket_tags",
                schema: "ticket");

            // Revert changes to tickets table
            migrationBuilder.DropColumn(
                name: "AssigneeId",
                schema: "ticket",
                table: "tickets");

            migrationBuilder.DropColumn(
                name: "ReporterId",
                schema: "ticket",
                table: "tickets");

            migrationBuilder.DropColumn(
                name: "category",
                schema: "ticket",
                table: "tickets");

            migrationBuilder.DropColumn(
                name: "closed_at",
                schema: "ticket",
                table: "tickets");

            migrationBuilder.DropColumn(
                name: "department",
                schema: "ticket",
                table: "tickets");

            migrationBuilder.DropColumn(
                name: "module_name",
                schema: "ticket",
                table: "tickets");

            migrationBuilder.DropColumn(
                name: "priority",
                schema: "ticket",
                table: "tickets");

            migrationBuilder.DropColumn(
                name: "region",
                schema: "ticket",
                table: "tickets");

            migrationBuilder.DropColumn(
                name: "resolved_at",
                schema: "ticket",
                table: "tickets");

            migrationBuilder.DropColumn(
                name: "title",
                schema: "ticket",
                table: "tickets");

            migrationBuilder.DropColumn(
                name: "MentionedUsers",
                schema: "ticket",
                table: "ticket_comments");

            migrationBuilder.AddColumn<string>(
                name: "subject",
                schema: "ticket",
                table: "tickets",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<TicketStatus>(
                name: "status",
                schema: "ticket",
                table: "tickets",
                type: "ticket.ticket_status",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(30)",
                oldMaxLength: 30);
        }
    }
}
