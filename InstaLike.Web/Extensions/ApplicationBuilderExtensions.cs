using InstaLike.Web.Infrastructure;
using Microsoft.AspNetCore.Builder;

namespace InstaLike.Web.Extensions
{
    internal static class ApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseRequestCorrelation(this IApplicationBuilder builder)
        {
            builder.UseMiddleware<RequestCorrelationMiddleware>();

            return builder;
        }
    }
}
