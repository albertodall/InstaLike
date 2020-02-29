using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using RT.Comb;
using Serilog.Context;

namespace InstaLike.Web.Infrastructure
{
    /// <summary>
    /// Adds a Correlation ID to every request, to ease activity tracking.
    /// </summary>
    internal sealed class RequestCorrelationMiddleware
    {
        private const string CorrelationIDPropertyName = "CorrelationID";

        private readonly RequestDelegate _next;

        public RequestCorrelationMiddleware(RequestDelegate next)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
        }

        public async Task Invoke(HttpContext context)
        {
            var correlationID = Provider.Sql.Create();
            using (LogContext.PushProperty(CorrelationIDPropertyName, correlationID))
            {
                await _next.Invoke(context);
            }
        }
    }
}
