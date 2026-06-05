using Adrenalin.Modules.Auth.Domain.Entities;
using Adrenalin.Modules.KB.Domain.Entities;
using Adrenalin.Modules.Lookup.Domain.Entities;
using Adrenalin.SharedKernel.Mediator;
using Microsoft.EntityFrameworkCore;

namespace Adrenalin.Persistence.Context;

public class AppDbContext : DbContext
{
    private readonly IPublisher _publisher;

    public AppDbContext(DbContextOptions<AppDbContext> options, IPublisher publisher)
        : base(options)
    {
        _publisher = publisher;
    }

    #region AUTH
    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<Group> Groups => Set<Group>();
    public DbSet<UserGroup> UserGroups => Set<UserGroup>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<UserSession> UserSessions => Set<UserSession>();
    public DbSet<TokenBlacklist> TokenBlacklists => Set<TokenBlacklist>();
    public DbSet<UserVerificationToken> UserVerificationTokens => Set<UserVerificationToken>();
    public DbSet<UserOtpCode> UserOtpCodes => Set<UserOtpCode>();
    #endregion

    #region LOOKUP
    public DbSet<CustomerTier> CustomerTiers => Set<CustomerTier>();
    public DbSet<GeoRegion> GeoRegions => Set<GeoRegion>();
    public DbSet<Module> Modules => Set<Module>();
    public DbSet<ProductVersion> ProductVersions => Set<ProductVersion>();
    public DbSet<SubModule> SubModules => Set<SubModule>();
    #endregion

    #region KNOWLEDGE BASE
    public DbSet<KbFolder> KbFolders => Set<KbFolder>();
    public DbSet<KbArticle> KbArticles => Set<KbArticle>();
    public DbSet<KbAttachment> KbAttachments => Set<KbAttachment>();
    public DbSet<PortalBanner> PortalBanners => Set<PortalBanner>();
    #endregion

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        foreach (var entity in builder.Model.GetEntityTypes())
        {
            var idProp = entity.FindProperty("Id");
            if (idProp != null && idProp.GetColumnName() == "Id")
                idProp.SetColumnName("id");
        }
    }

    /// <summary>
    /// Dispatches domain events from KB aggregates after every successful save.
    /// </summary>
    public override async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        var result = await base.SaveChangesAsync(ct);
        await DispatchDomainEventsAsync(ct);
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
