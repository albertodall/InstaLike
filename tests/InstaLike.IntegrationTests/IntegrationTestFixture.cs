using System.Threading.Tasks;
using MediatR;
using Xunit;

namespace InstaLike.IntegrationTests
{
    [Collection(TestInfrastructureFactory.CollectionName)]
    public abstract class IntegrationTestFixture : IAsyncLifetime
    {
        protected IntegrationTestFixture(TestInfrastructureFactory infrastructure)
        {
            Infrastructure = infrastructure;
        }

        protected TestInfrastructureFactory Infrastructure { get; }

        public async ValueTask InitializeAsync()
        {
            await Infrastructure.ResetAsync();
        }

        public ValueTask DisposeAsync() => ValueTask.CompletedTask;

        /// <summary>
        /// Sends the request in a session of its own, like a single request.
        /// </summary>
        protected async Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> request)
        {
            using var session = Infrastructure.OpenSession();
            return await new TestMediatorBuilder(session, Infrastructure.PictureStore).Build().Send(request, TestContext.Current.CancellationToken);
        }
    }
}
