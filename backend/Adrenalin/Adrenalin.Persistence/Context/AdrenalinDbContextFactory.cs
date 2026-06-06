using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;
using Npgsql;
using Adrenalin.Modules.Ticketing.Domain.Enums;
using Adrenalin.Persistence.Context;

namespace Adrenalin.Persistence.Context;

public class AdrenalinDbContextFactory : IDesignTimeDbContextFactory<AdrenalinDbContext>
{
    public AdrenalinDbContext CreateDbContext(string[] args)
    {
        // Get the directory of the startup project (unify.API)
        var basePath = Path.Combine(Directory.GetCurrentDirectory(), "..", "Adrenalin.unify.API");
        if (!Directory.Exists(basePath))
        {
            basePath = Directory.GetCurrentDirectory();
        }

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
        optionsBuilder.UseNpgsql(dataSource, o => o.MapEnum<TicketStatus>("ticket_status", "ticket"));

        return new AdrenalinDbContext(optionsBuilder.Options);
    }
}
