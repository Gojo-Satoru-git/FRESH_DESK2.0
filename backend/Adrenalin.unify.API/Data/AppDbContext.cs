

using Adrenalin.unify.API.Models.AuthModels;
using Adrenalin.unify.API.Models.Lookup;
using Microsoft.EntityFrameworkCore;


namespace Adrenalin.unify.API.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
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

    public DbSet<UserVerificationToken> UserVerificationTokens
        => Set<UserVerificationToken>();

    public DbSet<UserOtpCode> UserOtpCodes
        => Set<UserOtpCode>();

    #endregion

    #region LOOKUP

    public DbSet<CustomerTier> CustomerTiers
        => Set<CustomerTier>();

    public DbSet<GeoRegion> GeoRegions
        => Set<GeoRegion>();

    public DbSet<ProductModule> Modules
        => Set<ProductModule>();

    public DbSet<ProductVersion> ProductVersions
        => Set<ProductVersion>();

    public DbSet<SubModule> SubModules
        => Set<SubModule>();

    #endregion

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

       builder.ApplyConfigurationsFromAssembly(
    typeof(AppDbContext).Assembly);
    }
}