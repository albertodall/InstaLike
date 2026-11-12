using InstaLike.Web.Extensions;
using InstaLike.Web.Media;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using NHibernate;
using Serilog;

namespace InstaLike.IntegrationTests;

internal sealed class TestMediatorBuilder
{
    private readonly IServiceCollection _services = new ServiceCollection();
    private IPictureStore _pictureStore = new InMemoryPictureStore();

    public TestMediatorBuilder(ISession session)
    {
        _services.AddSingleton(session);
        _services.AddSingleton(Log.Logger);
        _services.ConfigureMediator();
    }

    public TestMediatorBuilder WithPictureStore(IPictureStore pictureStore)
    {
        _pictureStore = pictureStore;
        return this;
    }

    /// <summary>
    /// Adds a handler type, such as a test-only command handler.
    /// </summary>
    public TestMediatorBuilder WithHandler<THandler, TImplementation>()
        where THandler : class
        where TImplementation : class, THandler
    {
        _services.AddTransient<THandler, TImplementation>();
        return this;
    }

    /// <summary>
    /// Adds a handler instance, such as one that records the events it receives.
    /// </summary>
    public TestMediatorBuilder WithHandler<THandler>(THandler handler)
        where THandler : class
    {
        _services.AddSingleton(handler);
        return this;
    }

    /// <summary>
    /// The sender that dispatches requests to the registered handlers.
    /// </summary>
    public ISender Build()
    {
        _services.AddSingleton(_pictureStore);
        return _services.BuildServiceProvider().CreateScope().ServiceProvider.GetRequiredService<ISender>();
    }
}
