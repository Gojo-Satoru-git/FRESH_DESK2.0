using Adrenalin.SharedKernel.Interfaces;
using Adrenalin.SharedKernel.Mediator;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Adrenalin.UnitTests;

public class DispatcherTests
{
    public class FakeUnitOfWork : IUnitOfWork
    {
        public bool SaveChangesAsyncCalled { get; private set; }

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            SaveChangesAsyncCalled = true;
            return Task.FromResult(1);
        }
    }

    public record PingCommand(string Message) : IRequest<string>;

    public class PingHandler : IRequestHandler<PingCommand, string>
    {
        public Task<string> Handle(PingCommand request, CancellationToken cancellationToken)
        {
            return Task.FromResult($"Pong: {request.Message}");
        }
    }

    public record PingQuery(string Message) : IRequest<string>;

    public class PingQueryHandler : IRequestHandler<PingQuery, string>
    {
        public Task<string> Handle(PingQuery request, CancellationToken cancellationToken)
        {
            return Task.FromResult($"Pong Query: {request.Message}");
        }
    }

    public class LogBehavior : IPipelineBehavior<PingCommand, string>
    {
        public static List<string> Order { get; } = new();

        public async Task<string> Handle(
            PingCommand request, 
            RequestHandlerDelegate<string> next, 
            CancellationToken cancellationToken)
        {
            Order.Add("LogStart");
            var res = await next();
            Order.Add("LogEnd");
            return res;
        }
    }

    public class AuthBehavior : IPipelineBehavior<PingCommand, string>
    {
        public async Task<string> Handle(
            PingCommand request, 
            RequestHandlerDelegate<string> next, 
            CancellationToken cancellationToken)
        {
            LogBehavior.Order.Add("AuthStart");
            var res = await next();
            LogBehavior.Order.Add("AuthEnd");
            return res;
        }
    }

    [Fact]
    public async Task Send_ShouldRouteToHandler_AndExecuteBehaviorsInRussianDollOrder()
    {
        // Arrange
        LogBehavior.Order.Clear();
        var services = new ServiceCollection();
        
        // Scan SharedKernel assembly (registers CustomDispatcher and its components)
        services.AddCustomDispatcher(typeof(IDispatcher).Assembly);

        // Register fake unit of work
        var fakeUnitOfWork = new FakeUnitOfWork();
        services.AddSingleton<IUnitOfWork>(fakeUnitOfWork);

        // Manually register handler and behaviors in specific order
        services.AddTransient<IRequestHandler<PingCommand, string>, PingHandler>();
        services.AddTransient<IPipelineBehavior<PingCommand, string>, LogBehavior>();
        services.AddTransient<IPipelineBehavior<PingCommand, string>, AuthBehavior>();

        var provider = services.BuildServiceProvider();
        var dispatcher = provider.GetRequiredService<IDispatcher>();

        // Act
        var result = await dispatcher.Send(new PingCommand("Hello"));

        // Assert
        Assert.Equal("Pong: Hello", result);
        Assert.Equal(new[] { "LogStart", "AuthStart", "AuthEnd", "LogEnd" }, LogBehavior.Order);
        Assert.True(fakeUnitOfWork.SaveChangesAsyncCalled);
    }

    [Fact]
    public async Task Send_ShouldNotCallUnitOfWork_WhenRequestIsQuery()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddCustomDispatcher(typeof(IDispatcher).Assembly);

        var fakeUnitOfWork = new FakeUnitOfWork();
        services.AddSingleton<IUnitOfWork>(fakeUnitOfWork);

        services.AddTransient<IRequestHandler<PingQuery, string>, PingQueryHandler>();

        var provider = services.BuildServiceProvider();
        var dispatcher = provider.GetRequiredService<IDispatcher>();

        // Act
        var result = await dispatcher.Send(new PingQuery("Hello"));

        // Assert
        Assert.Equal("Pong Query: Hello", result);
        Assert.False(fakeUnitOfWork.SaveChangesAsyncCalled);
    }

    [Fact]
    public void AssemblyScanning_ShouldRegisterHandlersAndBehaviors()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddCustomDispatcher(Assembly.GetExecutingAssembly());
        var provider = services.BuildServiceProvider();

        // Assert
        // Verify handler registered
        var handler = provider.GetService<IRequestHandler<PingCommand, string>>();
        Assert.NotNull(handler);
        Assert.IsType<PingHandler>(handler);

        // Verify behaviors registered
        var behaviors = provider.GetServices<IPipelineBehavior<PingCommand, string>>();
        Assert.Contains(behaviors, b => b is LogBehavior);
        Assert.Contains(behaviors, b => b is AuthBehavior);
    }
}
