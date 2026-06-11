using Adrenalin.Infrastructure.Authentication;
using Adrenalin.Infrastructure.Storage;
using Adrenalin.Modules.Auth.Application.Commands;
using Adrenalin.Modules.Auth.Domain.Interfaces;
using Adrenalin.Modules.Ticketing.Application;
using Adrenalin.Modules.Ticketing.Domain.Enums;
using Adrenalin.Persistence;
using Adrenalin.Persistence.Context;
using Adrenalin.Persistence.DependencyInjection;
using Adrenalin.SharedKernel.Behaviors;
using Adrenalin.SharedKernel.Interfaces;
using Adrenalin.SharedKernel.Mediator;
using Adrenalin.unify.API.Authorization;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Npgsql;
using Scalar.AspNetCore;
using Adrenalin.Modules.Auth.Domain.Enums;
using System.Text;
using Adrenalin.Persistence.Repositories;
using Adrenalin.unify.API.Middlewares;
using Adrenalin.Infrastructure.Email;

var builder = WebApplication.CreateBuilder(args);

// ── 1. Database — single AdrenalinDbContext ───────────────────────────────────
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
dataSourceBuilder.MapEnum<TicketStatus>("ticket.ticket_status");
dataSourceBuilder.MapEnum<RevocationReason>("auth.revocation_reason");
dataSourceBuilder.EnableUnmappedTypes();
var dataSource = dataSourceBuilder.Build();

builder.Services.AddDbContext<AdrenalinDbContext>(options =>
    options.UseNpgsql(dataSource,
        npgsql => npgsql
            .MigrationsAssembly("Adrenalin.Persistence")
            .MapEnum<TicketStatus>("ticket_status", "ticket")
            .MapEnum<RevocationReason>("revocation_reason", "auth")
        )
        .UseSnakeCaseNamingConvention()
    );

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

builder.Services.AddValidatorsFromAssembly(
    typeof(Adrenalin.Modules.Auth.Application.Validators.CreateRoleCommandValidator).Assembly);

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
                            jwtSection["SecretKey"]!)),
                ClockSkew = TimeSpan.Zero
            };
    });
builder.Services.Configure<JwtOptions>(
    builder.Configuration.GetSection("Jwt"));

builder.Services.AddScoped<
    IJwtProvider,
    JwtProvider>();
builder.Services.AddScoped<
    IRefreshTokenGenerator,
    RefreshTokenGenerator>();

builder.Services.AddScoped<
    ITokenHasher,
    TokenHasher>();
builder.Services.AddScoped<
    IRefreshTokenRepository,
    RefreshTokenRepository>();
// ── 5. All repositories — single extension ───────────────────────────────────
builder.Services.AddPersistence();

// ── 6. Module application layer registrations ────────────────────────────────
builder.Services.AddTicketingApplication();
builder.Services.AddKbModule();

// ── RabbitMQ EventBus ──────────────────────────────────────────────────────────
var rabbitMqEnabled = builder.Configuration.GetValue<bool>("RabbitMQ:Enabled", true);
if (rabbitMqEnabled)
{
    builder.Services.AddSingleton<Adrenalin.EventBus.IEventBus, Adrenalin.EventBus.RabbitMQEventBus>();
    builder.Services.AddHostedService<Adrenalin.EventBus.RabbitMQConsumerService>();
}
else
{
    builder.Services.AddSingleton<Adrenalin.EventBus.IEventBus, Adrenalin.EventBus.InMemoryEventBus>();
}

// ── Email Polling & Receiving ────────────────────────────────────────────────
builder.Services.AddSingleton<Adrenalin.Infrastructure.Email.IEmailReceive, Adrenalin.Infrastructure.Email.ImapEmailReceiver>();
builder.Services.AddHostedService<Adrenalin.unify.API.BackgroundJobs.EmailPollingJob>();

// Integration Event Handlers
builder.Services.AddScoped<Adrenalin.EventBus.IIntegrationEventHandler<Adrenalin.EventBus.Events.TicketCreatedIntegrationEvent>, Adrenalin.Modules.Notification.Application.IntegrationEvents.TicketCreatedNotificationHandler>();
builder.Services.AddScoped<Adrenalin.EventBus.IIntegrationEventHandler<Adrenalin.EventBus.Events.TicketAssignedIntegrationEvent>, Adrenalin.Modules.Notification.Application.IntegrationEvents.TicketAssignedNotificationHandler>();
builder.Services.AddScoped<Adrenalin.EventBus.IIntegrationEventHandler<Adrenalin.EventBus.Events.TicketResolvedIntegrationEvent>, Adrenalin.Modules.Notification.Application.IntegrationEvents.TicketResolvedNotificationHandler>();
builder.Services.AddScoped<Adrenalin.EventBus.IIntegrationEventHandler<Adrenalin.EventBus.Events.TicketClosedIntegrationEvent>, Adrenalin.Modules.Notification.Application.IntegrationEvents.TicketClosedNotificationHandler>();
builder.Services.AddScoped<Adrenalin.EventBus.IIntegrationEventHandler<Adrenalin.EventBus.Events.TicketCommentAddedIntegrationEvent>, Adrenalin.Modules.Notification.Application.IntegrationEvents.TicketCommentAddedNotificationHandler>();

// ── 7. Auth infrastructure ───────────────────────────────────────────────────
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();

// ── 8. Shared infrastructure ─────────────────────────────────────────────────
builder.Services.AddScoped<IFileStorageService, LocalFileStorageService>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, Adrenalin.unify.API.Services.CurrentUserService>();
builder.Services.AddScoped<IUserVerificationTokenRepository,UserVerificationTokenRepository>();

builder.Services.AddScoped<IEmailService,FakeEmailService>();
// ── 9. Controllers + OpenAPI ─────────────────────────────────────────────────
builder.Services.AddControllers().AddJsonOptions(options =>
    options.JsonSerializerOptions.Converters.Add(
        new System.Text.Json.Serialization.JsonStringEnumConverter()));

builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();


builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "Adrenalin Unify API",
        Description = "API for Adrenalin Unify, the all-in-one ITSM platform.",
        Contact = new OpenApiContact
        {
            Name = "Adrenalin Software",
            Email = "info@adrenalin.com"
        }
    });

    options.AddSecurityDefinition("bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "JWT Authorization header using the Bearer scheme."
    });
    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("bearer", document)] = []
    });

});

// ── 10. Auth/authz ───────────────────────────────────────────────────────────
builder.Services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
builder.Services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();
builder.Services.AddAuthorization();

// ── 11. Exception handling — single handler via IExceptionHandler ─────────────
builder.Services.AddExceptionHandler<Adrenalin.unify.API.Infrastructure.GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularDevClient",
        policyBuilder => policyBuilder
            .WithOrigins("http://localhost:4200")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials());
});

var app = builder.Build();

if (args.Contains("--seed"))
{
    using (var scope = app.Services.CreateScope())
    {
        var context = scope.ServiceProvider.GetRequiredService<AdrenalinDbContext>();
        var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
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
        o.SwaggerEndpoint("/swagger/v1/swagger.json", "Adrenalin Unify API v1");
        o.RoutePrefix = "swagger";
    });
}
app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseHttpsRedirection();

app.UseCors("AllowAngularDevClient");
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