using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using RT.Comb;
using Serilog;
using Serilog.Context;

namespace InstaLike.Web.Infrastructure
{
    internal sealed class RequestLoggingProcessor<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    {
        private const string CorrelationIDPropertyName = "CorrelationID";

        private static Guid CorrelationID = Guid.Empty;

        private readonly ILogger _logger;
        private readonly Stopwatch _chronometer = new Stopwatch();

        public RequestLoggingProcessor(ILogger logger)
        {
            _logger = logger?.ForContext<TRequest>() ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<TResponse> next)
        {
            CorrelationID = Provider.Sql.Create();
            using (LogContext.PushProperty(CorrelationIDPropertyName, CorrelationID))
            {
                _chronometer.Start();
                _logger.Debug("Start Request");
                var response = await next();
                _chronometer.Stop();
                _logger.Debug($"End Request. Elapsed time: {_chronometer.ElapsedMilliseconds} ms.");
                return response;
            }
        }
    }
}
