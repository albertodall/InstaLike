using System;
using System.Threading.Tasks;
using InstaLike.Web.Services;
using Microsoft.AspNetCore.Http;
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
        private readonly ISequentialGuidGenerator _guidGenerator;

        public RequestCorrelationMiddleware(RequestDelegate next, ISequentialGuidGenerator guidGenerator)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _guidGenerator = guidGenerator ?? throw new ArgumentNullException(nameof(guidGenerator));
        }

        public async Task Invoke(HttpContext context)
        {
            var correlationID = _guidGenerator.GetNextId();
            using (LogContext.PushProperty(CorrelationIDPropertyName, correlationID))
            {
                await _next.Invoke(context);
            }
        }
    }
}
