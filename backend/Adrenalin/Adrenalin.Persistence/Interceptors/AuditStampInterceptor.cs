using Adrenalin.SharedKernel.Entities;
using Adrenalin.SharedKernel.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Adrenalin.Persistence.Interceptors;

public sealed class AuditStampInterceptor : SaveChangesInterceptor
{
    private readonly ICurrentUserService _currentUser;

    public AuditStampInterceptor(ICurrentUserService currentUser)
    {
        _currentUser = currentUser;
    }

    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        StampEntities(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        StampEntities(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void StampEntities(DbContext? context)
    {
        if (context is null) return;

        var now = DateTimeOffset.UtcNow;
        var userId = _currentUser.UserId;

        foreach (var entry in context.ChangeTracker.Entries<AuditableEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    if (entry.Entity.CreatedAt == default)
                        entry.Property(nameof(AuditableEntity.CreatedAt)).CurrentValue = now;

                    if (entry.Entity.CreatedBy is null && userId.HasValue)
                        entry.Property(nameof(AuditableEntity.CreatedBy)).CurrentValue = userId;

                    entry.Property(nameof(AuditableEntity.UpdatedAt)).CurrentValue = now;
                    break;

                case EntityState.Modified:
                    entry.Property(nameof(AuditableEntity.CreatedAt)).IsModified = false;
                    entry.Property(nameof(AuditableEntity.CreatedBy)).IsModified = false;

                    entry.Property(nameof(AuditableEntity.UpdatedAt)).CurrentValue = now;

                    if (userId.HasValue)
                        entry.Property(nameof(AuditableEntity.UpdatedBy)).CurrentValue = userId;
                    break;
            }
        }
    }
}
