using System;
using System.Threading;
using System.Threading.Tasks;
using InstaLike.Web.Services;
using MediatR;
using Serilog;
using Serilog.Context;

namespace InstaLike.Web.Infrastructure
{
    internal class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    {
        private const string CorrelationIdPropertyName = "CorrelationID";

        private readonly ILogger _logger;
        private readonly ISequentialGuidGeneratorService _correlationIDGenerator;

        public LoggingBehavior(ILogger logger, ISequentialGuidGeneratorService correlationIDGenerator)
        {
            _logger = logger?.ForContext<TRequest>() ?? throw new ArgumentNullException(nameof(logger));
            _correlationIDGenerator = correlationIDGenerator ?? throw new ArgumentNullException(nameof(correlationIDGenerator)); ;
        }

        public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<TResponse> next)
        {
            var correlationId = _correlationIDGenerator.GetIdentifier();
            using (LogContext.PushProperty(CorrelationIdPropertyName, _correlationIDGenerator.GetIdentifier()))
            {
                _logger.Debug("Started Request");
                var response = await next();
                _logger.Debug("Ended Request");
                return response;
            }
        }
    }
}
