using Adrenalin.Persistence.Context;
using Adrenalin.SharedKernel.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Adrenalin.IntegrationTests;

public abstract class BaseIntegrationTest : IClassFixture<IntegrationTestWebAppFactory>
{
    protected readonly IntegrationTestWebAppFactory Factory;
    protected readonly IServiceScope Scope;
    protected readonly AdrenalinDbContext DbContext;
    protected readonly IUnitOfWork UnitOfWork;

    protected BaseIntegrationTest(IntegrationTestWebAppFactory factory)
    {
        Factory = factory;
        Scope = factory.Services.CreateScope();
        DbContext = Scope.ServiceProvider.GetRequiredService<AdrenalinDbContext>();
        UnitOfWork = Scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
    }
}
