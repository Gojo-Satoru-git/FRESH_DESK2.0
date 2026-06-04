using Adrenalin.Infrastructure.Storage;
using Adrenalin.Modules.KB.Application.Services;
using Adrenalin.Modules.KB.Application.Validators;
using Adrenalin.Modules.KB.Domain.Interfaces;
using Adrenalin.Persistence.Context;
using Adrenalin.Persistence.Repositories.KnowledgeBase;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace Adrenalin.Persistence.DependencyInjection;

/// <summary>
/// Registers all KB repositories, validators, and services.
/// Call builder.Services.AddKbModule() from Program.cs.
/// MediatR handlers are picked up by MediatR's assembly scan automatically.
/// </summary>
public static class KbServiceRegistration
{
    public static IServiceCollection AddKbModule(this IServiceCollection services)
    {
        // Repositories
        services.AddScoped<IKbFolderRepository,    KbFolderRepository>();
        services.AddScoped<IKbArticleRepository,   KbArticleRepository>();
        services.AddScoped<IPortalBannerRepository, PortalBannerRepository>();

        // File storage — saves uploads to wwwroot/kb-attachments/ on the server.
        // Replace LocalKbFileStorageService with a cloud implementation
        // (e.g. S3KbFileStorageService) here without any other code changes.
        services.AddScoped<IKbFileStorageService, LocalKbFileStorageService>();

        // FluentValidation — scans Application assembly
        services.AddValidatorsFromAssemblyContaining<CreateKbArticleCommandValidator>();

        return services;
    }
}
