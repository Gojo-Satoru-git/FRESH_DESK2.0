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
using Adrenalin.Persistence.Repositories.Auth;
using Adrenalin.Modules.Company.Domain.Interfaces;
using Adrenalin.EventBus;
using Adrenalin.EventBus.Events;
using Adrenalin.Modules.AI;
using Adrenalin.Modules.Company.Applications.EventHandlers;
using Adrenalin.Modules.Company.Applications.Commands;
using Adrenalin.Modules.Auth.Application.Notifications;
using Adrenalin.Modules.SLA.Application;
using Adrenalin.Persistence.Interceptors;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHealthChecks()
    .AddDbContextCheck<Adrenalin.Persistence.Context.AdrenalinDbContext>("DatabaseHealthCheck")
    .AddCheck<Adrenalin.unify.API.HealthChecks.RoutingHealthCheck>("RoutingHealthCheck");

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);

dataSourceBuilder.MapEnum<TicketStatus>("ticket.ticket_status");
dataSourceBuilder.MapEnum<TicketPriority>("ticket.ticket_priority");
dataSourceBuilder.MapEnum<TicketSource>("ticket.ticket_source");
dataSourceBuilder.MapEnum<RevocationReason>("auth.revocation_reason");
dataSourceBuilder.EnableUnmappedTypes();


dataSourceBuilder.MapEnum<RevocationReason>(
    "auth.revocation_reason");
var dataSource = dataSourceBuilder.Build();
builder.Services.AddScoped<AuditStampInterceptor>();
builder.Services.AddScoped<LookupCacheInvalidationInterceptor>();
builder.Services.AddDbContext<AdrenalinDbContext>(
    (sp, options) =>
    {
        options.UseNpgsql(
            dataSource,
            npgsql => npgsql
                .MigrationsAssembly("Adrenalin.Persistence")
                .MapEnum<TicketStatus>("ticket_status", "ticket")
                .MapEnum<TicketPriority>("ticket_priority", "ticket")
                .MapEnum<TicketSource>("ticket_source", "ticket")
                .MapEnum<RevocationReason>("revocation_reason", "auth")
        );

        options.UseSnakeCaseNamingConvention();

        options.AddInterceptors(
            sp.GetRequiredService<AuditStampInterceptor>(),
            sp.GetRequiredService<LookupCacheInvalidationInterceptor>());
    });
    builder.Services.AddRateLimiter(options =>
{
     options.AddPolicy("LoginPolicy", _ =>
        RateLimitPartition.GetFixedWindowLimiter(
            "login",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 5,
                Window = TimeSpan.FromMinutes(1)
            }));
        
        options.AddPolicy("RefreshPolicy", _ =>
        RateLimitPartition.GetFixedWindowLimiter(
            "refresh",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 20,
                Window = TimeSpan.FromMinutes(1)
            }));

         options.AddPolicy("ForgotPasswordPolicy", _ =>
        RateLimitPartition.GetFixedWindowLimiter(
            "forgot",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 3,
                Window = TimeSpan.FromMinutes(15)
            }));

    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
});

// ── 2. Custom MediatR dispatcher — scan ALL module assemblies once ────────────
builder.Services.AddScoped<
    Adrenalin.Modules.Ticketing.Application.Services.IEmailThreadDetectionService,
    Adrenalin.Modules.Ticketing.Application.Services.EmailThreadDetectionService>();

builder.Services.AddScoped<
    Adrenalin.Modules.Ticketing.Application.Services.IEmailWatcherExtractionService,
    Adrenalin.Modules.Ticketing.Application.Services.EmailWatcherExtractionService>();

builder.Services.AddCustomDispatcher(
    typeof(Adrenalin.Modules.Auth.Application.Commands.RegisterUserCommand).Assembly,
    typeof(Adrenalin.Modules.Ticketing.Application.Commands.Tickets.CreateTicketCommand).Assembly,
    typeof(Adrenalin.Modules.KB.Application.Commands.CreateKbArticleCommand).Assembly,
    typeof(CreateContactForExternalUserCommand).Assembly,
    typeof(ExternalUserCreatedNotificationHandler).Assembly, 
    typeof(Adrenalin.Modules.SLA.Application.Commands.CheckEscalationsCommand).Assembly,
    typeof(Adrenalin.Modules.Notification.Application.Queries.GetUnreadNotificationsQuery).Assembly);


// ── 3. Pipeline behaviors (order matters — outermost registered first) ────────
// Validation runs first, then UnitOfWork commits on success
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(UnitOfWorkBehavior<,>));

// ── 4. FluentValidation — scan all validator assemblies ──────────────────────
builder.Services.AddValidatorsFromAssembly(
    typeof(Adrenalin.Modules.Auth.Application.Validators.RegisterUserValidator).Assembly);
builder.Services.AddValidatorsFromAssembly(
    typeof(Adrenalin.Modules.Ticketing.Application.Commands.Tickets.CreateTicketCommand).Assembly);
builder.Services.AddValidatorsFromAssembly(
    typeof(Adrenalin.Modules.KB.Application.Commands.CreateKbArticleCommand).Assembly);

builder.Services.AddValidatorsFromAssembly(
    typeof(Adrenalin.Modules.Auth.Application.Validators.CreateRoleCommandValidator).Assembly);

builder.Services.AddValidatorsFromAssembly(
    typeof(CreateContactForExternalUserCommand).Assembly);

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
builder.Services.AddScoped<
    IOtpGenerator,
    OtpGenerator>();
builder.Services.AddScoped<
    IUserOtpCodeRepository,
    UserOtpCodeRepository>();
builder.Services.AddScoped<
    IPasswordGenerator,
    PasswordGenerator>();
    builder.Services.AddScoped<
    IContactRepository,
    ContactRepository>();
builder.Services.AddScoped<
    IUserSessionRepository,
    UserSessionRepository>();
// ── 5. All repositories — single extension ───────────────────────────────────
builder.Services.AddPersistence();

// ── 6. Module application layer registrations ────────────────────────────────

builder.Services.AddAiModule();
builder.Services.AddTicketingApplication();
builder.Services.AddKbModule();

// ── RabbitMQ EventBus ──────────────────────────────────────────────────────────
var rabbitMqEnabled = builder.Configuration.GetValue<bool>("RabbitMQ:Enabled", true);
builder.Services.Configure<EmailSettings>(
    builder.Configuration.GetSection("Email"));
if (rabbitMqEnabled)
{
    builder.Services.AddSingleton<Adrenalin.EventBus.RabbitMQEventBus>();
    builder.Services.AddScoped<Adrenalin.EventBus.IEventBus, Adrenalin.Persistence.Outbox.OutboxEventBus>();
    builder.Services.AddHostedService<Adrenalin.EventBus.RabbitMQConsumerService>();
    builder.Services.AddHostedService<Adrenalin.Persistence.Outbox.OutboxProcessorBackgroundService>();
}
else
{
    builder.Services.AddSingleton<Adrenalin.EventBus.IEventBus, Adrenalin.EventBus.InMemoryEventBus>();
}

// ── Email Polling & Receiving ────────────────────────────────────────────────
builder.Services.AddSingleton<Adrenalin.Infrastructure.Email.Inbound.IInboundEmailProvider, Adrenalin.Infrastructure.Email.Inbound.ImapInboundProvider>();
builder.Services.AddSingleton<Adrenalin.Infrastructure.Email.Inbound.IInboundEmailProvider, Adrenalin.Infrastructure.Email.Inbound.WebhookInboundProvider>();
builder.Services.AddSingleton<Adrenalin.Infrastructure.Email.Inbound.IInboundEmailProvider, Adrenalin.Infrastructure.Email.Inbound.MicrosoftGraphInboundProvider>();
builder.Services.AddHostedService<Adrenalin.unify.API.BackgroundJobs.EmailIngestionBackgroundService>();
builder.Services.AddHostedService<Adrenalin.unify.API.BackgroundJobs.NotificationTemplateValidatorService>();

// Integration Event Handlers
builder.Services.AddScoped<Adrenalin.EventBus.IIntegrationEventHandler<Adrenalin.EventBus.Events.EmailReceivedIntegrationEvent>, Adrenalin.Modules.Ticketing.Application.IntegrationEvents.ProcessInboundEmailIntegrationEventHandler>();
builder.Services.AddScoped<Adrenalin.EventBus.IIntegrationEventHandler<Adrenalin.EventBus.Events.TicketCreatedIntegrationEvent>, Adrenalin.Modules.Notification.Application.IntegrationEvents.TicketCreatedNotificationHandler>();
builder.Services.AddScoped<Adrenalin.EventBus.IIntegrationEventHandler<Adrenalin.EventBus.Events.TicketAssignedIntegrationEvent>, Adrenalin.Modules.Notification.Application.IntegrationEvents.TicketAssignedNotificationHandler>();
builder.Services.AddScoped<Adrenalin.EventBus.IIntegrationEventHandler<Adrenalin.EventBus.Events.TicketResolvedIntegrationEvent>, Adrenalin.Modules.Notification.Application.IntegrationEvents.TicketResolvedNotificationHandler>();
builder.Services.AddScoped<Adrenalin.EventBus.IIntegrationEventHandler<Adrenalin.EventBus.Events.TicketClosedIntegrationEvent>, Adrenalin.Modules.Notification.Application.IntegrationEvents.TicketClosedNotificationHandler>();
builder.Services.AddScoped<Adrenalin.EventBus.IIntegrationEventHandler<Adrenalin.EventBus.Events.TicketCommentAddedIntegrationEvent>, Adrenalin.Modules.Notification.Application.IntegrationEvents.TicketCommentAddedNotificationHandler>();
builder.Services.AddScoped<Adrenalin.EventBus.IIntegrationEventHandler<Adrenalin.EventBus.Events.SlaBreachedIntegrationEvent>, Adrenalin.Modules.Notification.Application.IntegrationEvents.SlaNotificationHandler>();

// ── 7. Auth infrastructure ───────────────────────────────────────────────────
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();

// ── 8. Shared infrastructure ─────────────────────────────────────────────────
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetSection("Redis")["ConnectionString"];
    options.InstanceName = "Adrenalin_";
});
builder.Services.AddSingleton<ICacheService, Adrenalin.Infrastructure.Cache.RedisCacheService>();
builder.Services.AddMemoryCache();

builder.Services.AddSingleton<Adrenalin.Infrastructure.Storage.Providers.IFileStorageProvider, Adrenalin.Infrastructure.Storage.LocalFileStorageProvider>();
builder.Services.AddSingleton<IFileStorageService, Adrenalin.Infrastructure.Storage.FileStorageResolver>();
builder.Services.AddScoped<Adrenalin.SharedKernel.Interfaces.IAttachmentIntegrityService, Adrenalin.Persistence.Services.AttachmentIntegrityService>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, Adrenalin.unify.API.Services.CurrentUserService>();
builder.Services.AddScoped<IUserVerificationTokenRepository,UserVerificationTokenRepository>();
builder.Services.AddScoped<IRoleRepository, RoleRepository>();
builder.Services.AddScoped<IUserRoleRepository, UserRoleRepository>();
builder.Services.AddScoped<Adrenalin.SharedKernel.Interfaces.IIntegrationEventLogRepository, IntegrationEventLogRepository>();
builder.Services.AddScoped<
    IEmailService,
    SmtpEmailService>();
builder.Services.AddScoped<
    IIntegrationEventHandler<ExternalUserCreatedEvent>,
    ExternalUserCreatedEventHandler>();
// ── 9. Controllers + OpenAPI ─────────────────────────────────────────────────
builder.Services.AddControllers()
    .AddJsonOptions(options =>
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
// SLA module
builder.Services.AddSLAApplication();

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
app.UseMiddleware<SessionActivityMiddleware>();
app.UseAuthorization();
app.UseRateLimiter();

// Static files for KB attachments
var rawKbPath = builder.Configuration["KbStorage:BasePath"];
if (string.IsNullOrWhiteSpace(rawKbPath))
{
    rawKbPath = Path.Combine(builder.Environment.ContentRootPath, "uploads", "kb-attachments");
}
var kbStoragePath = Path.GetFullPath(rawKbPath);

if (!Directory.Exists(kbStoragePath))
    Directory.CreateDirectory(kbStoragePath);

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(kbStoragePath),
    RequestPath = "/kb-attachments"
});

// Static files for ticket attachments
var rawAttachmentsPath = builder.Configuration["FileStorage:LocalPath"];
if (string.IsNullOrWhiteSpace(rawAttachmentsPath))
{
    rawAttachmentsPath = Path.Combine(builder.Environment.ContentRootPath, "uploads");
}
var attachmentsStoragePath = Path.GetFullPath(rawAttachmentsPath);

if (!Directory.Exists(attachmentsStoragePath))
    Directory.CreateDirectory(attachmentsStoragePath);

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(attachmentsStoragePath),
    RequestPath = "" // since fileUrl starts with 'attachments/', mapping root serves them at /attachments/...
});

app.MapControllers();
app.Run();

public partial class Program { }