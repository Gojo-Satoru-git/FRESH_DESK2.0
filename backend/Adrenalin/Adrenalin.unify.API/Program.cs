using Adrenalin.Persistence.Context;
using Adrenalin.Persistence;
using Adrenalin.Modules.Ticketing.Application;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Adrenalin.Modules.Ticketing.Domain.Enums;
using Adrenalin.SharedKernel.Interfaces;
using Adrenalin.Infrastructure.Storage;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers().AddJsonOptions(
    options =>{ options.JsonSerializerOptions.Converters.Add(
            new System.Text.Json.Serialization.JsonStringEnumConverter()
        );
    });

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
dataSourceBuilder.MapEnum<TicketStatus>("ticket.ticket_status");
dataSourceBuilder.EnableUnmappedTypes();
var dataSource = dataSourceBuilder.Build();

builder.Services.AddDbContext<AdrenalinDbContext>(options =>
{
    options.UseNpgsql(dataSource, o => o.MapEnum<TicketStatus>("ticket_status", "ticket"));
});

builder.Services.AddTicketingPersistence();
builder.Services.AddTicketingApplication();
builder.Services.AddScoped<IFileStorageService, LocalFileStorageService>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, Adrenalin.unify.API.Services.CurrentUserService>();
builder.Services.AddExceptionHandler<Adrenalin.unify.API.Infrastructure.GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

var app = builder.Build();

app.UseExceptionHandler();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "Adrenalin Unify API v1");
        options.RoutePrefix = "swagger";
    });
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
