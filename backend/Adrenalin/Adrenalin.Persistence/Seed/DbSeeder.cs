using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Adrenalin.Persistence.Context;
using Adrenalin.Modules.Auth.Domain.Entities;
using Adrenalin.Modules.Auth.Domain.Interfaces;
using Adrenalin.Modules.Company.Domain.Entities;
using Adrenalin.Modules.Gamification.Domain.Entities;
using Adrenalin.Modules.KB.Domain.Entities;
using Adrenalin.Modules.SLA.Domain.Entities;
using Adrenalin.Modules.Lookup.Domain.Entities;
using Adrenalin.Modules.Notification.Domain.Entities;
using Adrenalin.Modules.AI.Domain.Entities;
using Adrenalin.Modules.Workflow.Domain.Entities;
using Adrenalin.Modules.Ticketing.Domain.Entities;
using Adrenalin.Modules.Ticketing.Domain.Enums;

namespace Adrenalin.Persistence.Seed
{
    public static class DbSeeder
    {
        public static async Task SeedAsync(AdrenalinDbContext context, IPasswordHasher passwordHasher)
        {
            Console.WriteLine("Starting database seeding of 10 sample records for all tables...");

            // Define topological order of seeding to satisfy foreign key constraints.
            var orderedTypes = new List<Type>
            {
                // 1. Role / Permissions / Auth
                typeof(Role),
                typeof(Permission),
                typeof(RolePermission),
                typeof(User),
                typeof(UserRole),
                
                // 2. Lookup Tables & Workflow Graphs
                typeof(GeoRegion),
                typeof(Holiday),
                typeof(BusinessHour),
                typeof(CustomerTier),
                typeof(SolutionType),
                typeof(ProductVersion),
                typeof(TicketStatusGraph),
                typeof(TicketStatusGraphScope),
                typeof(StatusTransition),
                typeof(CustomerStatusMap),

                // 3. Company
                typeof(Company),
                typeof(CompanyContactsLimit),
                typeof(CompanyDomain),
                typeof(Contact),

                // 4. Groups
                typeof(Group),
                typeof(UserGroup),

                // 5. Auth details
                typeof(RefreshToken),
                typeof(UserSession),
                typeof(UserOtpCode),
                typeof(UserVerificationToken),
                typeof(TokenBlacklist),
                typeof(AuditLog),

                // 6. Workflow
                typeof(Adrenalin.Modules.Lookup.Domain.Entities.Module),
                typeof(SubModule),

                // 7. Gamification
                typeof(Badge),
                typeof(Challenge),
                typeof(PointRule),
                typeof(AgentBadge),
                typeof(AgentChallenge),
                typeof(AgentPoint),
                typeof(AgentStreak),
                typeof(LeaderboardSnapshot),

                // 8. SLA
                typeof(SlaPolicy),
                typeof(AutomationRule),

                // 9. KnowledgeBase
                typeof(KbFolder),
                typeof(KbArticle),
                typeof(KbAttachment),
                typeof(PortalBanner),

                // 10. Ticketing
                typeof(Ticket),
                typeof(SlaTicket),
                typeof(EscalationRule),
                typeof(TicketAttachment),
                typeof(TicketComment),
                typeof(TicketCustomField),
                typeof(TicketRelation),
                typeof(TicketWatcher),
                typeof(TicketRiskScore),
                typeof(TicketStatusHistory),
                typeof(TicketAssignmentLog),
                typeof(TicketClassification),
                typeof(CsatSurvey),

                // 11. Logs and Notifications
                typeof(NotificationTemplate),
                typeof(NotificationLog),
                typeof(AutomationExecutionLog),
                typeof(AutoResolutionLog),
                typeof(AiSuggestionLog)
            };

            // Map each Type to its unique table index for deterministic Guids
            var tableIndices = new Dictionary<Type, int>();
            for (int i = 0; i < orderedTypes.Count; i++)
            {
                tableIndices[orderedTypes[i]] = i + 1;
            }

            // Execute seeding table by table
            foreach (var type in orderedTypes)
            {
                Console.WriteLine($"Seeding table: {type.Name}...");

                var entityType = context.Model.FindEntityType(type);
                if (entityType == null)
                {
                    Console.WriteLine($"Warning: Entity type metadata not found for {type.Name}. Skipping.");
                    continue;
                }

                int tableIndex = tableIndices[type];

                // Find primary key property
                var primaryKeyProp = entityType.FindPrimaryKey()?.Properties.FirstOrDefault();
                if (primaryKeyProp == null) continue;

                // Check if 10 records already exist for this table using our deterministic IDs
                bool alreadySeeded = true;
                for (int rowIndex = 0; rowIndex < 10; rowIndex++)
                {
                    var recordId = GetPrimaryKeyValue(primaryKeyProp, tableIndex, rowIndex);
                    var existing = await context.FindAsync(type, recordId);
                    if (existing == null)
                    {
                        alreadySeeded = false;
                        break;
                    }
                }

                if (alreadySeeded)
                {
                    Console.WriteLine($"Table {type.Name} already fully seeded. Skipping.");
                    continue;
                }

                var list = new List<object>();

                for (int rowIndex = 0; rowIndex < 10; rowIndex++)
                {
                    var entity = Activator.CreateInstance(type, true);
                    if (entity == null)
                    {
                        Console.WriteLine($"Failed to instantiate {type.Name}");
                        continue;
                    }

                    // Populate scalar properties
                    foreach (var property in entityType.GetProperties())
                    {
                        if (property.IsShadowProperty()) continue;

                        // Primary key handling
                        if (property.IsPrimaryKey())
                        {
                            var recordId = GetPrimaryKeyValue(property, tableIndex, rowIndex);
                            SetProperty(entity, property.Name, recordId);
                            continue;
                        }

                        // Foreign key handling
                        var fk = property.GetContainingForeignKeys().FirstOrDefault();
                        if (fk != null)
                        {
                            var principalType = fk.PrincipalEntityType.ClrType;
                            if (property.Name == "CreatedBy" || property.Name == "UpdatedBy")
                            {
                                SetProperty(entity, property.Name, null);
                            }
                            else if (tableIndices.TryGetValue(principalType, out int principalTableIndex))
                            {
                                var principalKeyProp = fk.PrincipalKey.Properties.FirstOrDefault();
                                if (principalKeyProp != null)
                                {
                                    if (principalType == type) // self-referencing (e.g. parent folder)
                                    {
                                        if (rowIndex == 0)
                                        {
                                            SetProperty(entity, property.Name, null);
                                        }
                                        else
                                        {
                                            SetProperty(entity, property.Name, GetPrimaryKeyValue(principalKeyProp, principalTableIndex, 0));
                                        }
                                    }
                                    else if (principalTableIndex > tableIndex)
                                    {
                                        if (property.IsNullable)
                                        {
                                            SetProperty(entity, property.Name, null);
                                        }
                                    }
                                    else
                                    {
                                        // Offset to create variation
                                        int offset = (property.Name.Contains("Assignee") || 
                                                      property.Name.Contains("UpdatedBy") || 
                                                      property.Name.Contains("Author")) ? 1 : 0;
                                        
                                        var targetVal = GetPrimaryKeyValue(principalKeyProp, principalTableIndex, (rowIndex + offset) % 10);
                                        SetProperty(entity, property.Name, targetVal);
                                    }
                                }
                            }
                            else if (property.IsNullable)
                            {
                                SetProperty(entity, property.Name, null);
                            }
                            continue;
                        }

                        // Unique constraints / specific fields
                        bool isUnique = property.DeclaringEntityType.GetIndexes().Any(idx => idx.IsUnique && idx.Properties.Contains(property));
                        if (isUnique && property.ClrType == typeof(string))
                        {
                            string prefix = property.Name;
                            string val = $"{prefix}_{rowIndex}_{Guid.NewGuid().ToString("N").Substring(0, 8)}";
                            var maxLen = property.GetMaxLength();
                            if (maxLen.HasValue && val.Length > maxLen.Value)
                            {
                                val = val.Substring(val.Length - maxLen.Value);
                            }
                            SetProperty(entity, property.Name, val);
                            continue;
                        }

                        // JSON StoreType check
                        var storeType = property.GetColumnType();
                        if (storeType != null && (storeType.Contains("json", StringComparison.OrdinalIgnoreCase) || storeType.Contains("jsonb", StringComparison.OrdinalIgnoreCase)))
                        {
                            SetProperty(entity, property.Name, "{}");
                            continue;
                        }

                        if (property.ClrType == typeof(string) && property.Name.Contains("Email", StringComparison.OrdinalIgnoreCase))
                        {
                            string email = $"sample.{property.Name.ToLowerInvariant()}{rowIndex}@adrenalin-dev.com";
                            if (property.Name.StartsWith("Normalized", StringComparison.OrdinalIgnoreCase))
                                email = email.ToUpperInvariant();
                            SetProperty(entity, property.Name, email);
                            continue;
                        }

                        if (property.Name.Contains("Username", StringComparison.OrdinalIgnoreCase))
                        {
                            string username = $"user_{rowIndex}";
                            if (property.Name.StartsWith("Normalized", StringComparison.OrdinalIgnoreCase))
                                username = username.ToUpperInvariant();
                            SetProperty(entity, property.Name, username);
                            continue;
                        }

                        if (property.Name.Equals("PasswordHash", StringComparison.OrdinalIgnoreCase))
                        {
                            SetProperty(entity, property.Name, passwordHasher.Hash("Password123!"));
                            continue;
                        }

                        if (property.Name.Contains("Phone", StringComparison.OrdinalIgnoreCase) || 
                            property.Name.Contains("Mobile", StringComparison.OrdinalIgnoreCase))
                        {
                            SetProperty(entity, property.Name, $"+1555010{rowIndex}");
                            continue;
                        }

                        if (property.Name.Contains("Url", StringComparison.OrdinalIgnoreCase))
                        {
                            SetProperty(entity, property.Name, $"https://example.com/assets/{property.Name.ToLowerInvariant()}_{rowIndex}.png");
                            continue;
                        }

                        if (property.Name.Contains("Domain", StringComparison.OrdinalIgnoreCase) && property.ClrType == typeof(string))
                        {
                            SetProperty(entity, property.Name, $"company{rowIndex}.com");
                            continue;
                        }

                        // General type fallbacks
                        if (property.ClrType == typeof(string))
                        {
                            string val = $"Sample_{property.Name}_{rowIndex}";
                            var maxLen = property.GetMaxLength();
                            if (maxLen.HasValue && val.Length > maxLen.Value)
                            {
                                val = val.Substring(0, maxLen.Value);
                            }
                            SetProperty(entity, property.Name, val);
                        }
                        else if (property.ClrType == typeof(bool))
                        {
                            SetProperty(entity, property.Name, true);
                        }
                        else if (property.ClrType == typeof(int) || property.ClrType == typeof(int?))
                        {
                            SetProperty(entity, property.Name, rowIndex + 1);
                        }
                        else if (property.ClrType == typeof(long) || property.ClrType == typeof(long?))
                        {
                            SetProperty(entity, property.Name, (long)(rowIndex + 1));
                        }
                        else if (property.ClrType == typeof(decimal) || property.ClrType == typeof(decimal?))
                        {
                            SetProperty(entity, property.Name, 0.1m + (rowIndex * 0.05m));
                        }
                        else if (property.ClrType == typeof(double) || property.ClrType == typeof(double?))
                        {
                            SetProperty(entity, property.Name, 0.1 + (rowIndex * 0.05));
                        }
                        else if (property.ClrType == typeof(float) || property.ClrType == typeof(float?))
                        {
                            SetProperty(entity, property.Name, 0.1f + (rowIndex * 0.05f));
                        }
                        else if (property.ClrType == typeof(DateTimeOffset) || property.ClrType == typeof(DateTimeOffset?))
                        {
                            SetProperty(entity, property.Name, DateTimeOffset.UtcNow.AddDays(-rowIndex));
                        }
                        else if (property.ClrType == typeof(DateTime) || property.ClrType == typeof(DateTime?))
                        {
                            SetProperty(entity, property.Name, DateTime.UtcNow.AddDays(-rowIndex));
                        }
                        else if (property.ClrType == typeof(DateOnly) || property.ClrType == typeof(DateOnly?))
                        {
                            SetProperty(entity, property.Name, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-rowIndex)));
                        }
                        else if (property.ClrType == typeof(TimeOnly) || property.ClrType == typeof(TimeOnly?))
                        {
                            SetProperty(entity, property.Name, TimeOnly.FromDateTime(DateTime.UtcNow.AddHours(-rowIndex)));
                        }
                        else if (property.ClrType == typeof(IPAddress))
                        {
                            SetProperty(entity, property.Name, IPAddress.Parse($"127.0.0.{rowIndex + 1}"));
                        }
                        else if (property.ClrType == typeof(string[]))
                        {
                            SetProperty(entity, property.Name, new string[] { $"tag_{rowIndex}", "demo" });
                        }
                        else if (property.ClrType == typeof(List<string>))
                        {
                            SetProperty(entity, property.Name, new List<string> { $"tag_{rowIndex}", "demo" });
                        }
                        else if (property.ClrType.IsEnum)
                        {
                            var values = Enum.GetValues(property.ClrType);
                            if (values.Length > 0)
                            {
                                SetProperty(entity, property.Name, values.GetValue(rowIndex % values.Length));
                            }
                        }
                        else
                        {
                            var underlyingType = Nullable.GetUnderlyingType(property.ClrType);
                            if (underlyingType != null && underlyingType.IsEnum)
                            {
                                var values = Enum.GetValues(underlyingType);
                                if (values.Length > 0)
                                {
                                    SetProperty(entity, property.Name, values.GetValue(rowIndex % values.Length));
                                }
                            }
                        }
                    }

                    list.Add(entity);
                }

                // Add to Context and save
                try
                {
                    context.AddRange(list);
                    await context.SaveChangesAsync();
                    Console.WriteLine($"Successfully seeded {list.Count} records in {type.Name}.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error seeding {type.Name}: {ex.Message}");
                    if (ex.InnerException != null)
                    {
                        Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                    }
                    // Clean up tracking of failed entities to avoid polluting the next saves
                    foreach (var entry in context.ChangeTracker.Entries().ToList())
                    {
                        entry.State = EntityState.Detached;
                    }
                }
            }

            Console.WriteLine("Database seeding completed!");
        }

        private static Guid GetGuid(int tableIndex, int rowIndex)
        {
            return new Guid($"00000000-0000-0000-0000-{tableIndex:D4}{rowIndex:D8}");
        }

        private static object GetPrimaryKeyValue(IProperty keyProperty, int tableIndex, int rowIndex)
        {
            if (keyProperty.ClrType == typeof(Guid))
            {
                return GetGuid(tableIndex, rowIndex);
            }
            if (keyProperty.ClrType == typeof(int) || keyProperty.ClrType == typeof(int?))
            {
                return rowIndex + 1;
            }
            if (keyProperty.ClrType == typeof(long) || keyProperty.ClrType == typeof(long?))
            {
                return (long)(rowIndex + 1);
            }
            if (keyProperty.ClrType == typeof(string))
            {
                return $"key_{tableIndex}_{rowIndex}";
            }
            return rowIndex + 1;
        }

        private static void SetProperty(object entity, string propertyName, object? value)
        {
            var type = entity.GetType();
            PropertyInfo? prop = null;
            while (type != null)
            {
                prop = type.GetProperty(propertyName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
                if (prop != null) break;
                type = type.BaseType;
            }

            if (prop != null)
            {
                if (value != null && prop.PropertyType.IsEnum)
                {
                    if (value is string strVal)
                        value = Enum.Parse(prop.PropertyType, strVal, true);
                    else
                        value = Enum.ToObject(prop.PropertyType, value);
                }
                else if (value != null && Nullable.GetUnderlyingType(prop.PropertyType) is Type underlying && underlying.IsEnum)
                {
                    if (value is string strVal)
                        value = Enum.Parse(underlying, strVal, true);
                    else
                        value = Enum.ToObject(underlying, value);
                }

                prop.SetValue(entity, value);
            }
        }
    }
}
