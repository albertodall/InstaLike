using InstaLike.Web.Infrastructure;
using InstaLike.Web.Media;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using NHibernate;
using Serilog;

namespace InstaLike.IntegrationTests
{
    /// <summary>
    /// Builds the application's MediatR pipeline, with its handlers and behaviors such as the unit of work,
    /// on the test's session, so tests can send requests as the application does without starting it.
    /// </summary>
    internal sealed class TestMediatorBuilder
    {
        private readonly IServiceCollection _services = new ServiceCollection();

        /// <summary>
        /// Registers the application's handlers, working on the given session and picture store.
        /// </summary>
        public TestMediatorBuilder(ISession session, IPictureStore pictureStore)
        {
            _services.AddSingleton(session);
            _services.AddSingleton(pictureStore);
            _services.AddSingleton(Log.Logger);
            _services.ConfigureMediator();
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
        /// The sender that dispatches requests to the registered handlers.
        /// </summary>
        public ISender Build()
        {
            return _services.BuildServiceProvider()
                .CreateScope()
                .ServiceProvider.GetRequiredService<ISender>();
        }
    }
}
