using System.Linq;
using InstaLike.Core.Commands;
using InstaLike.Web.Data.Query;
using InstaLike.Web.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace InstaLike.Web.Extensions
{
    internal static class ServiceCollectionExtensions
    {
        public static IServiceCollection RegisterCommandHandlers(this IServiceCollection services)
        {
            services.Scan(scanner => scanner
                .FromAssemblyOf<IMessageDispatcher>()
                    .AddClasses(classes => classes.AssignableTo(typeof(ICommandHandler<>)))
                    .AsImplementedInterfaces());

            return services;
        }

        public static IServiceCollection RegisterQueryHandlers(this IServiceCollection services)
        {
            services.Scan(scanner => scanner
                .FromAssemblyOf<IMessageDispatcher>()
                    .AddClasses(classes => classes.AssignableTo(typeof(IQueryHandler<,>)))
                    .AsImplementedInterfaces());

            return services;
        }
    }
}
