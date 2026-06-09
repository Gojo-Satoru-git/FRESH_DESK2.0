using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Adrenalin.Persistence.Context;
using Adrenalin.Modules.Notification.Domain.Entities;

namespace Adrenalin.Persistence.Context;

public static class DbSeeder
{
    public static async Task SeedTemplatesAsync(AdrenalinDbContext context)
    {
        var templates = new (string Code, string Name, string Subject, string Body)[]
        {
            (
                "TICKET_ASSIGNED_AGENT",
                "Agent Notification / Ticket Assigned to Agent",
                "New Ticket with {{ticket.priority}} Priority for {{ticket.company.name}} - {{ticket.id}} has been Assigned.",
                "Hi {{ticket.agent.name}},\n\nA {{ticket.priority}} ticket ID [{{ticket.id}}] has been assigned to you. Please follow the link below to view the ticket.\nTicket URL: {{ticket.url}}\n\nTicket Information\n\nTicket ID\t{{ticket.id}}\nSubject\t{{ticket.subject}}\nDescription\t{{ticket.description}}\nPriority\t{{ticket.priority}}\nRaised By - Contact\t{{ticket.requester.name}} \n{{ticket.requester.phone}}\n{{ticket.from_email}}\n{{ticket.company.name}}"
            ),
            (
                "REQUESTER_REPLIED",
                "Agent Notification / Requester Replies to Ticket",
                "{{ticket.agent.name}} - An update has been registered for your Ticket ID: {{ticket.id}}",
                "Hi {{ticket.agent.name}},\n\nThe customer has responded to the ticket - {{ticket.id}}.\n\nTicket Subject:\n{{ticket.subject}}\n\nTicket comment\n{{comment.body}}\n\n\n{{ticket.url}}"
            ),
            (
                "UNATTENDED_IN_GROUP",
                "Agent Notification / Ticket Unattended in Group",
                "Unattended ticket - {{ticket.id}} for {{ticket.company.name}}",
                "Hi {{agent.name}},\n\nThis is an escalation email for the {{ticket.group.name}} group in {{helpdesk_name}}.\n\nA new ticket in {{ticket.group.name}} group is currently unassigned for more than {{ticket.group.assign_time_mins}} minutes.\n\nTicket Details:\n\nSubject - {{ticket.subject}}\n\nTicket URL - {{ticket.url}}"
            ),
            (
                "FIRST_RESPONSE_SLA_VIOLATION",
                "Agent Notification / First Response SLA Violation",
                "Ticket - {{ticket.id}} - Response time SLA violated - {{ticket.company.name}}",
                "Hi,\n\nThere has been no response for a ticket {{ticket.id}} which has been tagged as {{ticket.priority}}. The first response was due by {{ticket.fr_due_by_hrs}} today.\n\nTicket Details:\n\nSubject - {{ticket.subject}}\n\nClient: {{ticket.company.name}}\n\nRequestor - {{ticket.requester.email}}\n\nThis is an escalation email from {{helpdesk_name}}\n{{ticket.url}}"
            ),
            (
                "RESOLUTION_SLA_VIOLATION",
                "Agent Notification / Resolution Time SLA Violation",
                "Resolution time SLA violated - {{ticket.subject}}",
                "Hi,\n\nTicket has not been resolved within the SLA time period. The ticket was due by {{ticket.due_by_hrs}} today.\n\nTicket Details:\n\nSubject - {{ticket.subject}}\n\nRequestor - {{ticket.requester.email}}\n\nAgent - {{ticket.agent.name}}\n\nThis is an escalation email from {{helpdesk_name}}\n{{ticket.url}}"
            ),
            (
                "NOTE_ADDED_TO_TICKET",
                "Agent Notification / Note added to ticket",
                "Note Added - [#{{ticket.id}}] {{ticket.subject}}",
                "Hi ,\n\n{{comment.commenter.name}} added a note and wants you to have a look.\n\nTicket URL:\n{{ticket.url}}\n\nSubject:\n{{ticket.subject}}\n\nRequester: {{ticket.requester.name}}\n\nNote Content:\n{{comment.body}}"
            ),
            (
                "FIRST_RESPONSE_SLA_REMINDER",
                "Agent Notification / First Response SLA Reminder",
                "Response due for {{ticket.subject}}",
                "Hi,\n\nResponse is due for ticket #{{ticket.id}}.\n\nTicket Details:\n\nSubject - {{ticket.subject}}\n\nRequestor - {{ticket.requester.email}}\n\nTicket link - {{ticket.url}}\n\nThis is a reminder email from {{helpdesk_name}}"
            ),
            (
                "RESOLUTION_SLA_REMINDER",
                "Agent Notification / Resolution Time SLA Reminder",
                "Resolution expected - {{ticket.subject}}",
                "Hi,\n\nResolution time for ticket #{{ticket.id}} is fast approaching. The ticket is due by {{ticket.due_by_hrs}}.\n\nTicket Details:\n\nSubject - {{ticket.subject}}\n\nRequestor - {{ticket.requester.email}}\n\nTicket link - {{ticket.url}}\n\nThis is a reminder email from {{helpdesk_name}}"
            ),
            (
                "TICKET_CREATED_REQUESTER",
                "Requester Notification / New Ticket Created",
                "Ticket [{{ticket.id}}] has been posted- {{ticket.subject}}",
                "Dear {{ticket.requester.name}},\nWe would like to acknowledge that we have received your request and a ticket has been created.\nA support representative will be reviewing your request and reach you on your query. \nPlease note the below tips to get a faster resolution:\n1.\t Explain the issue faced with clear artifacts (user journey, Business scenario of an issue, Business Requirement explanation for feature recommendations etc.)\n2.\t Provide supporting documents and information. Ex: to report a leave application issue, please quote the form name, impacted username – ID, screenshot of the error.\n3.\tIn case if it is payroll computation issue, mention what is the expected value or formulae\nTo view the status of the ticket or add comments, please visit\n\n{{ticket.url}}\nThank you for your patience.\nSincerely,\nAdrenalin Support Team"
            ),
            (
                "AGENT_ADDED_COMMENT",
                "Requester Notification / Agent Adds Comment to Ticket",
                "Adrenalin: An update has been registered for Ticket No: {{ticket.id}}",
                "Hi{{ticket.requester.name}},\nAn update was submitted for this ticket ID: {{ticket.id}}\n-----------------------------Update Details------------------------------------------------\nUpdate Description: {{ticket.latest_public_comment}}\n \nUpdated by: {{ticket.agent.name}}\n\nTicket URL: {{ticket.url}}"
            ),
            (
                "TICKET_RESOLVED_REQUESTER",
                "Requester Notification / Agent Solves the Ticket",
                "Ticket Resolved - {{ticket.subject}}",
                "Dear {{ticket.requester.name}},\n\nOur Support Rep has indicated that your ticket has been {{ticket.status}}.\n\nIf you believe that the ticket has not been resolved, please reply to this email to automatically reopen the ticket.\nIf there is no response from you, we will assume that the ticket has been resolved and the ticket will be automatically closed after 15 days.\n\nSincerely,\n{{helpdesk_name}}Support Team\n{{ticket.url}}"
            ),
            (
                "TICKET_CLOSED_REQUESTER",
                "Requester Notification / Agent Closes the Ticket",
                "Ticket Closed - {{ticket.subject}}",
                "Dear {{ticket.requester.name}},\n\nWe understand that the below mentioned ticket has been closed.\nTicket ID - {{ticket.id}}\nSubject - {{ticket.subject}}\nDescription - {{ticket.description}}\n\nWe hope that the ticket was resolved to your satisfaction. If you feel that the ticket should not be closed or if the ticket has not been resolved, please reply to this email.\n\nSincerely,\n{{helpdesk_name}} Support Team\n{{ticket.url}}"
            ),
            (
                "USER_ACTIVATION",
                "Requester Notification / User Activation Email",
                "{{portal_name}} user activation",
                "Hi {{contact.name}},\n\nA new {{helpdesk_name}} account has been created for you.\n\nClick the url below to activate your account and select a password!\n\n{{activation_url}}\n\nIf the above URL does not work try copying and pasting it into your browser. If you continue to have problems, please feel free to contact us.\n\nRegards,\n{{helpdesk_name}}"
            )
        };

        foreach (var t in templates)
        {
            var existing = await context.NotificationTemplates
                .FirstOrDefaultAsync(x => x.Code == t.Code);

            if (existing != null)
            {
                // Update existing
                // Since property setters are private, we can use EF Core property entry updates or reflections
                context.Entry(existing).Property(x => x.Name).CurrentValue = t.Name;
                context.Entry(existing).Property(x => x.Subject).CurrentValue = t.Subject;
                context.Entry(existing).Property(x => x.BodyHtml).CurrentValue = t.Body;
                context.Entry(existing).Property(x => x.IsActive).CurrentValue = true;
                context.Entry(existing).Property(x => x.IsDeleted).CurrentValue = false;
            }
            else
            {
                // Create new
                var newTemplate = new NotificationTemplate();
                context.Entry(newTemplate).Property(x => x.Code).CurrentValue = t.Code;
                context.Entry(newTemplate).Property(x => x.Name).CurrentValue = t.Name;
                context.Entry(newTemplate).Property(x => x.Subject).CurrentValue = t.Subject;
                context.Entry(newTemplate).Property(x => x.BodyHtml).CurrentValue = t.Body;
                context.Entry(newTemplate).Property(x => x.IsActive).CurrentValue = true;
                context.Entry(newTemplate).Property(x => x.IsDeleted).CurrentValue = false;
                context.Entry(newTemplate).Property(x => x.CreatedAt).CurrentValue = DateTimeOffset.UtcNow;
                context.NotificationTemplates.Add(newTemplate);
            }
        }

        await context.SaveChangesAsync();
    }

    public static async Task SeedRolesAndPermissionsAsync(AdrenalinDbContext context, Adrenalin.Modules.Auth.Domain.Interfaces.IPasswordHasher hasher)
    {
        var adminUserId = Guid.Parse("00000000-0000-0000-0000-000000000001");

        var adminUserExists = await context.Users.AnyAsync(u => u.Id == adminUserId);
        if (!adminUserExists)
        {
            var adminUser = new Adrenalin.Modules.Auth.Domain.Entities.User();
            context.Entry(adminUser).Property(x => x.Id).CurrentValue = adminUserId;
            context.Entry(adminUser).Property(x => x.Email).CurrentValue = "admin@adrenalin.com";
            context.Entry(adminUser).Property(x => x.NormalizedEmail).CurrentValue = "ADMIN@ADRENALIN.COM";
            context.Entry(adminUser).Property(x => x.Username).CurrentValue = "admin";
            context.Entry(adminUser).Property(x => x.NormalizedUsername).CurrentValue = "ADMIN";
            context.Entry(adminUser).Property(x => x.PasswordHash).CurrentValue = hasher.Hash("Admin@123");
            context.Entry(adminUser).Property(x => x.FirstName).CurrentValue = "System";
            context.Entry(adminUser).Property(x => x.LastName).CurrentValue = "Admin";
            context.Entry(adminUser).Property(x => x.IsActive).CurrentValue = true;
            context.Entry(adminUser).Property(x => x.IsDeleted).CurrentValue = false;
            context.Entry(adminUser).Property(x => x.EmailVerified).CurrentValue = true;
            
            context.Users.Add(adminUser);
            await context.SaveChangesAsync();
        }

        // 1. Seed Permissions
        var permissionsToSeed = new (string Resource, string Action, string Description)[]
        {
            ("ticket", "create", "Create Tickets"),
            ("ticket", "read", "Read Tickets"),
            ("ticket", "update", "Update Tickets"),
            ("ticket", "delete", "Delete Tickets"),
            ("ticket", "assign", "Assign Tickets"),
            ("ticket", "comment", "Comment on Tickets"),
            ("ticket", "close", "Close Tickets"),
            ("ticket", "reopen", "Reopen Tickets"),
            ("rbac", "user:read", "Read RBAC Users"),
            ("rbac", "user:assign", "Assign RBAC Roles"),
            ("rbac", "role:read", "Read RBAC Roles"),
            ("rbac", "role:create", "Create RBAC Roles"),
            ("rbac", "role:update", "Update RBAC Roles"),
            ("rbac", "role:delete", "Delete RBAC Roles"),
            ("rbac", "permission:manage", "Manage RBAC Permissions"),
            ("rbac", "group:read", "Read RBAC Groups"),
            ("rbac", "group:manage", "Manage RBAC Groups")
        };

        var seededPermissions = new List<Adrenalin.Modules.Auth.Domain.Entities.Permission>();
        foreach (var p in permissionsToSeed)
        {
            var existing = await context.Permissions
                .FirstOrDefaultAsync(x => x.Resource == p.Resource && x.Action == p.Action);

            if (existing == null)
            {
                var newPerm = Adrenalin.Modules.Auth.Domain.Entities.Permission.Create(p.Resource, p.Action, p.Description, adminUserId);
                context.Permissions.Add(newPerm);
                seededPermissions.Add(newPerm);
            }
            else
            {
                seededPermissions.Add(existing);
            }
        }
        await context.SaveChangesAsync();

        // 2. Seed Roles
        var rolesToSeed = new (string Name, string Description, string[] AllowedPermissions)[]
        {
            ("Admin", "System Administrator", new[] { 
                "ticket:create", "ticket:read", "ticket:update", "ticket:delete", "ticket:assign", "ticket:comment", "ticket:close", "ticket:reopen", 
                "rbac:user:read", "rbac:user:assign", "rbac:role:read", "rbac:role:create", "rbac:role:update", "rbac:role:delete", 
                "rbac:permission:manage", "rbac:group:read", "rbac:group:manage" 
            }),
            ("Supervisor", "Team Lead / Supervisor", new[] { 
                "ticket:create", "ticket:read", "ticket:update", "ticket:assign", "ticket:comment", "ticket:close", "ticket:reopen",
                "rbac:user:read"
            }),
            ("Agent", "Support Agent", new[] { 
                "ticket:create", "ticket:read", "ticket:update", "ticket:comment" 
            }),
            ("Customer", "End Customer", new[] { 
                "ticket:create", "ticket:read", "ticket:comment", "ticket:update", "ticket:close", "ticket:reopen" 
            })
        };

        foreach (var r in rolesToSeed)
        {
            var existingRole = await context.Roles.FirstOrDefaultAsync(x => x.Name == r.Name);
            Guid roleId;
            if (existingRole == null)
            {
                var newRole = Adrenalin.Modules.Auth.Domain.Entities.Role.Create(r.Name, r.Description, adminUserId);
                context.Roles.Add(newRole);
                roleId = newRole.Id;
            }
            else
            {
                roleId = existingRole.Id;
            }
            await context.SaveChangesAsync();

            // Assign permissions
            foreach (var permKey in r.AllowedPermissions)
            {
                var parts = permKey.Split(':');
                var resource = parts[0];
                var action = parts[1];

                var dbPerm = seededPermissions.FirstOrDefault(p => p.Resource == resource && p.Action == action);
                if (dbPerm != null)
                {
                    var existingRp = await context.RolePermissions
                        .FirstOrDefaultAsync(rp => rp.RoleId == roleId && rp.PermissionId == dbPerm.Id);

                    if (existingRp == null)
                    {
                        var rp = Adrenalin.Modules.Auth.Domain.Entities.RolePermission.Assign(roleId, dbPerm.Id, adminUserId);
                        context.RolePermissions.Add(rp);
                    }
                    else if (existingRp.IsDeleted)
                    {
                        context.Entry(existingRp).Property(x => x.IsDeleted).CurrentValue = false;
                    }
                }
            }
        }
        await context.SaveChangesAsync();

        var adminRole = await context.Roles.FirstOrDefaultAsync(x => x.Name == "Admin");
        if (adminRole != null)
        {
            var adminUserRoleExists = await context.UserRoles.AnyAsync(ur => ur.UserId == adminUserId && ur.RoleId == adminRole.Id && !ur.IsDeleted);
            if (!adminUserRoleExists)
            {
                var ur = Adrenalin.Modules.Auth.Domain.Entities.UserRole.Assign(adminUserId, adminRole.Id, adminUserId);
                context.UserRoles.Add(ur);
                await context.SaveChangesAsync();
            }
        }

        // 3. Seed Regions and Departments (if missing)
        var regions = new[] { "SASIA", "MEA", "APAC" };
        foreach (var reg in regions)
        {
            var regExists = await context.GeoRegions.AnyAsync(r => r.Code == reg);
            if (!regExists)
            {
                var region = Adrenalin.Modules.Lookup.Domain.Entities.GeoRegion.Create(
                    code: reg,
                    label: $"{reg} Region",
                    timezone: "UTC",
                    businessStart: new TimeOnly(9, 0),
                    businessEnd: new TimeOnly(17, 0)
                );
                await context.GeoRegions.AddAsync(region);
            }
        }

        var depts = new[] { "Custom Solutions", "Delivery", "Product", "Support" };
        foreach (var dept in depts)
        {
            var moduleExists = await context.Modules.AnyAsync(m => m.Label == dept || m.Code == dept.ToUpperInvariant().Replace(" ", "_"));
            if (!moduleExists)
            {
                var module = Adrenalin.Modules.Lookup.Domain.Entities.Module.Create(
                    code: dept.ToUpperInvariant().Replace(" ", "_"),
                    label: dept,
                    department: dept
                );
                await context.Modules.AddAsync(module);
            }
        }
        await context.SaveChangesAsync();
    }
}
