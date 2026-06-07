using Adrenalin.Infrastructure.Authentication;
using Adrenalin.Infrastructure.Storage;
using Adrenalin.Modules.Auth.Domain.Interfaces;
using Adrenalin.Modules.Ticketing.Application;
using Adrenalin.Modules.Ticketing.Domain.Enums;
using Adrenalin.Persistence;
using Adrenalin.Persistence.Context;
using Adrenalin.Persistence.DependencyInjection;
using Adrenalin.SharedKernel.Behaviors;
using Adrenalin.SharedKernel.Interfaces;
using Adrenalin.SharedKernel.Mediator;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Npgsql;
using Scalar.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ── 1. Database — single AdrenalinDbContext ───────────────────────────────────
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
dataSourceBuilder.MapEnum<TicketStatus>("ticket.ticket_status");
dataSourceBuilder.EnableUnmappedTypes();
var dataSource = dataSourceBuilder.Build();

builder.Services.AddDbContext<AdrenalinDbContext>(options =>
    options.UseNpgsql(dataSource,
        npgsql => npgsql
            .MigrationsAssembly("Adrenalin.Persistence")
            .MapEnum<TicketStatus>("ticket_status", "ticket")));

// ── 2. Custom MediatR dispatcher — scan ALL module assemblies once ────────────
builder.Services.AddCustomDispatcher(
    typeof(Adrenalin.Modules.Auth.Application.Commands.RegisterUserCommand).Assembly,
    typeof(Adrenalin.Modules.Ticketing.Application.Commands.CreateTicketCommand).Assembly,
    typeof(Adrenalin.Modules.KB.Application.Commands.CreateKbArticleCommand).Assembly);

// ── 3. Pipeline behaviors (order matters — outermost registered first) ────────
// Validation runs first, then UnitOfWork commits on success
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(UnitOfWorkBehavior<,>));

// ── 4. FluentValidation — scan all validator assemblies ──────────────────────
builder.Services.AddValidatorsFromAssembly(
    typeof(Adrenalin.Modules.Auth.Application.Validators.RegisterUserValidator).Assembly);
builder.Services.AddValidatorsFromAssembly(
    typeof(Adrenalin.Modules.Ticketing.Application.Commands.CreateTicketCommand).Assembly);
builder.Services.AddValidatorsFromAssembly(
    typeof(Adrenalin.Modules.KB.Application.Commands.CreateKbArticleCommand).Assembly);

var jwtSection =
    builder.Configuration.GetSection("Jwt");

builder.Services
    .AddAuthentication(
        JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters =
            new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,

                ValidIssuer =
                    jwtSection["Issuer"],

                ValidAudience =
                    jwtSection["Audience"],

                IssuerSigningKey =
                    new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(
                            jwtSection["SecretKey"]!))
            };
    });
builder.Services.Configure<JwtOptions>(
    builder.Configuration.GetSection("Jwt"));

builder.Services.AddScoped<
    IJwtProvider,
    JwtProvider>();

// ── 5. All repositories — single extension ───────────────────────────────────
builder.Services.AddPersistence();

// ── 6. Module application layer registrations ────────────────────────────────
builder.Services.AddTicketingApplication();
builder.Services.AddKbModule();

// ── 7. Auth infrastructure ───────────────────────────────────────────────────
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();

// ── 8. Shared infrastructure ─────────────────────────────────────────────────
builder.Services.AddScoped<IFileStorageService, LocalFileStorageService>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, Adrenalin.unify.API.Services.CurrentUserService>();

// ── 9. Controllers + OpenAPI ─────────────────────────────────────────────────
builder.Services.AddControllers().AddJsonOptions(options =>
    options.JsonSerializerOptions.Converters.Add(
        new System.Text.Json.Serialization.JsonStringEnumConverter()));

builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ── 10. Auth/authz ───────────────────────────────────────────────────────────
builder.Services.AddAuthentication();
builder.Services.AddAuthorization(options =>
{
    options.DefaultPolicy = new Microsoft.AspNetCore.Authorization.AuthorizationPolicyBuilder()
        .RequireAssertion(_ => true)
        .Build();
    options.FallbackPolicy = null;
});

// ── 11. Exception handling — single handler via IExceptionHandler ─────────────
builder.Services.AddExceptionHandler<Adrenalin.unify.API.Infrastructure.GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

var app = builder.Build();

if (args.Contains("--seed"))
{
    using (var scope = app.Services.CreateScope())
    {
        var context = scope.ServiceProvider.GetRequiredService<AdrenalinDbContext>();
        var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
        await Adrenalin.Persistence.Seed.DbSeeder.SeedAsync(context, hasher);
    }
    return;
}

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();   // /scalar/v1
    app.UseSwagger();
    app.UseSwaggerUI(o =>
    {
        o.SwaggerEndpoint("/openapi/v1.json", "Adrenalin Unify API v1");
        o.RoutePrefix = "swagger";
    });
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

// Static files for KB attachments
var kbStoragePath = builder.Configuration["KbStorage:BasePath"]
    ?? Path.Combine(builder.Environment.ContentRootPath, "uploads", "kb-attachments");

if (!Directory.Exists(kbStoragePath))
    Directory.CreateDirectory(kbStoragePath);

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(kbStoragePath),
    RequestPath = "/kb-attachments"
});

app.MapControllers();
app.Run();