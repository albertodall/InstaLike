using System;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using InstaLike.Core.Commands;
using InstaLike.Web.Data.Query;
using Microsoft.Extensions.DependencyInjection;

namespace InstaLike.Web.Infrastructure
{
    internal sealed class MessageDispatcher : IMessageDispatcher
    {
        private readonly IServiceProvider _provider;

        public MessageDispatcher(IServiceProvider provider)
        {
            _provider = provider;
        }

        public async Task<Result> DispatchAsync(ICommand command)
        {
            Type type = typeof(ICommandHandler<>);
            Type[] typeArgs = { command.GetType() };
            Type handlerType = type.MakeGenericType(typeArgs);

            Result result;
            using (var scope = _provider.CreateScope())
            {
                dynamic handler = scope.ServiceProvider.GetRequiredService(handlerType);
                result = await handler.HandleAsync((dynamic)command);
            }
            return result;
        }

        public async Task<TResult> DispatchAsync<TResult>(IQuery<TResult> query)
        {
            Type type = typeof(IQueryHandler<,>);
            Type[] typeArgs = { query.GetType(), typeof(TResult) };
            Type handlerType = type.MakeGenericType(typeArgs);

            TResult result;
            using (var scope = _provider.CreateScope())
            {
                dynamic handler = scope.ServiceProvider.GetRequiredService(handlerType);
                result = await handler.HandleAsync((dynamic)query);
            }
            return result;
        }
    }
}
