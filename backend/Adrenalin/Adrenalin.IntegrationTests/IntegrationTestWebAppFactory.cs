using Microsoft.Extensions.Configuration;
using Adrenalin.Persistence.Context;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Testcontainers.PostgreSql;

namespace Adrenalin.IntegrationTests;

public class IntegrationTestWebAppFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _dbContainer;
    private string _connectionString = "";

    public IntegrationTestWebAppFactory()
    {
        _dbContainer = new PostgreSqlBuilder()
            .WithDatabase("Adrenalin-DB")
            .WithUsername("neondb_owner")
            .WithPassword("npg_0ShejQEoA4wP")
            .Build();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((context, configBuilder) =>
        {
            configBuilder.AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "ConnectionStrings:DefaultConnection", _connectionString }
            });
        });

        builder.ConfigureTestServices(services =>
        {
            // Override EventBus to avoid RabbitMQ connection issues during test
            services.RemoveAll(typeof(Adrenalin.EventBus.IEventBus));
            services.AddSingleton<Adrenalin.EventBus.IEventBus, Adrenalin.EventBus.InMemoryEventBus>();

            // Turn off background ingestion polling
            services.RemoveAll(typeof(Microsoft.Extensions.Hosting.IHostedService));
        });
    }

    public async Task InitializeAsync()
    {
        await _dbContainer.StartAsync();
        _connectionString = _dbContainer.GetConnectionString();

        using var scope = Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AdrenalinDbContext>();
        
            await dbContext.Database.ExecuteSqlRawAsync(
                @"CREATE SCHEMA IF NOT EXISTS ticket;
                  CREATE SCHEMA IF NOT EXISTS config;
                  
                  DROP TABLE IF EXISTS ticket.email_attachments CASCADE;
                  DROP TABLE IF EXISTS ticket.email_messages CASCADE;
                  DROP TABLE IF EXISTS ticket.processed_email_logs CASCADE;
                  DROP TABLE IF EXISTS config.email_alias_routes CASCADE;

                  CREATE TABLE IF NOT EXISTS ticket.email_messages (
                      id uuid NOT NULL PRIMARY KEY,
                      ticket_id uuid,
                      ticket_comment_id uuid,
                      provider text NOT NULL,
                      message_id text NOT NULL,
                      internet_message_id text NOT NULL,
                      thread_id text,
                      in_reply_to text,
                      ""references"" text,
                      sender_email text NOT NULL,
                      sender_name text NOT NULL,
                      recipient_email text NOT NULL,
                      cc_emails text[],
                      subject text NOT NULL,
                      body_html text,
                      body_text text,
                      received_at timestamp with time zone NOT NULL,
                      last_outbound_message_id text,
                      processing_state text NOT NULL,
                      created_at timestamp with time zone NOT NULL,
                      created_by uuid,
                      language text,
                      sender_domain text,
                      spam_score numeric,
                      email_classification text,
                      detected_intent text,
                      sentiment text,
                      is_deleted boolean NOT NULL DEFAULT false,
                      deleted_at timestamp with time zone,
                      deleted_by uuid,
                      updated_at timestamp with time zone,
                      updated_by uuid,
                      row_version bytea
                  );

                  CREATE TABLE IF NOT EXISTS ticket.email_attachments (
                      id uuid NOT NULL PRIMARY KEY,
                      email_message_id uuid NOT NULL REFERENCES ticket.email_messages(id),
                      ticket_attachment_id uuid,
                      file_name text NOT NULL,
                      content_type text NOT NULL,
                      size bigint NOT NULL,
                      hash text,
                      storage_path text,
                      created_at timestamp with time zone NOT NULL,
                      created_by uuid,
                      is_deleted boolean NOT NULL DEFAULT false,
                      deleted_at timestamp with time zone,
                      deleted_by uuid,
                      updated_at timestamp with time zone,
                      updated_by uuid,
                      row_version bytea
                  );

                  CREATE TABLE IF NOT EXISTS ticket.processed_email_logs (
                      id uuid NOT NULL PRIMARY KEY,
                      internet_message_id text NOT NULL UNIQUE,
                      provider text NOT NULL,
                      status text NOT NULL,
                      failure_reason text,
                      created_at timestamp with time zone NOT NULL,
                      created_by uuid,
                      is_deleted boolean NOT NULL DEFAULT false,
                      deleted_at timestamp with time zone,
                      deleted_by uuid,
                      updated_at timestamp with time zone,
                      updated_by uuid,
                      row_version bytea
                  );

                  CREATE TABLE IF NOT EXISTS config.email_alias_routes (
                      id uuid NOT NULL PRIMARY KEY,
                      email_address text NOT NULL,
                      group_id uuid NOT NULL,
                      company_id uuid,
                      priority integer NOT NULL,
                      is_active boolean NOT NULL DEFAULT true,
                      created_at timestamp with time zone NOT NULL DEFAULT timezone('utc', now()),
                      created_by uuid,
                      is_deleted boolean NOT NULL DEFAULT false,
                      deleted_at timestamp with time zone,
                      deleted_by uuid,
                      updated_at timestamp with time zone,
                      updated_by uuid,
                      row_version bytea
                  );
                "
            );
    }

    new public async Task DisposeAsync()
    {
        await _dbContainer.StopAsync();
        await _dbContainer.DisposeAsync();
    }
}
