using Adrenalin.Modules.KB.Application.Commands;
using Adrenalin.Persistence.Context;
using Adrenalin.Persistence.DependencyInjection;
using Adrenalin.SharedKernel.Results;
using Adrenalin.unify.API.Pipeline;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Scalar.AspNetCore;
using System.Reflection;


var builder = WebApplication.CreateBuilder(args);

// ── 1. DbContext ──────────────────────────────────────────────────────────────
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        npgsql => npgsql.MigrationsAssembly("Adrenalin.Persistence")));

// ── 2. MediatR ────────────────────────────────────────────────────────────────
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(CreateKbArticleCommand).Assembly);
});

// ── 3. FluentValidation pipeline ──────────────────────────────────────────────
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationPipelineBehaviour<,>));

// ── 4. KB module ──────────────────────────────────────────────────────────────
builder.Services.AddKbModule();

// ── 5. Controllers + OpenAPI (Scalar — compatible with .NET 10) ───────────────
builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddAuthentication();
builder.Services.AddAuthorization(options =>
{
    options.DefaultPolicy = new Microsoft.AspNetCore.Authorization.AuthorizationPolicyBuilder()
        .RequireAssertion(_ => true)
        .Build();
    options.FallbackPolicy = null;
});
var app = builder.Build();

// ── 6. Middleware ─────────────────────────────────────────────────────────────
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    // Scalar UI at /scalar/v1
    app.MapScalarApiReference();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

var kbStoragePath = builder.Configuration["KbStorage:BasePath"];

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(kbStoragePath!),
    RequestPath = "/kb-attachments"
});


try
{
    app.MapControllers();
}
catch (ReflectionTypeLoadException ex)
{
    Console.WriteLine("ReflectionTypeLoadException: " + ex.Message);
    Console.WriteLine("Types that were loaded (null for failed):");
    foreach (var t in ex.Types)
    {
        Console.WriteLine(t?.FullName ?? "<null>");
    }
    Console.WriteLine("Loader exceptions:");
    foreach (var le in ex.LoaderExceptions)
    {
        Console.WriteLine(le.GetType().FullName + ": " + le.Message);
        Console.WriteLine(le.ToString());
    }
    // rethrow so normal behavior remains
    throw;
}
app.Run();


