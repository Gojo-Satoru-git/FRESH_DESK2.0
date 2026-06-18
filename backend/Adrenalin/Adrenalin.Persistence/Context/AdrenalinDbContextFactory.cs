using Adrenalin.Modules.Ticketing.Domain.Enums;
using Adrenalin.Persistence.Context;
using Adrenalin.SharedKernel.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace Adrenalin.Persistence.Context;

public class AdrenalinDbContextFactory : IDesignTimeDbContextFactory<AdrenalinDbContext>
{
    public AdrenalinDbContext CreateDbContext(string[] args)
    {
        var basePath = Path.Combine(Directory.GetCurrentDirectory(), "..", "Adrenalin.unify.API");
        if (!Directory.Exists(basePath))
            basePath = Directory.GetCurrentDirectory();

        var configuration = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = configuration.GetConnectionString("DefaultConnection");

        var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
        dataSourceBuilder.MapEnum<TicketStatus>("ticket.ticket_status");
        dataSourceBuilder.EnableUnmappedTypes();
        var dataSource = dataSourceBuilder.Build();

        var optionsBuilder = new DbContextOptionsBuilder<AdrenalinDbContext>();
        optionsBuilder
            .UseNpgsql(dataSource, o => o.MapEnum<TicketStatus>("ticket_status", "ticket"))
            .UseSnakeCaseNamingConvention();

        return new AdrenalinDbContext(optionsBuilder.Options, new NoOpPublisher());
    }

    /// <summary>No-op publisher used only at design time (EF CLI migrations).</summary>
    private sealed class NoOpPublisher : Adrenalin.SharedKernel.Mediator.IPublisher
    {
        public Task Publish<TNotification>(TNotification notification,
            CancellationToken cancellationToken = default)
            where TNotification : Adrenalin.SharedKernel.Mediator.INotification
            => Task.CompletedTask;

        public Task Publish(Adrenalin.SharedKernel.Mediator.INotification notification,
            CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }
    private sealed class DesignTimeCurrentUserService : ICurrentUserService
{
    public Guid? UserId => null;

    public string? Email => null;

    public bool IsAuthenticated => false;

    public IEnumerable<string> Roles =>
        Enumerable.Empty<string>();
}
}
