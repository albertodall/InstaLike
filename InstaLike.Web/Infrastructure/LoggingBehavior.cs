using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Serilog;

namespace InstaLike.Web.Infrastructure
{
    internal class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    {
        private readonly ILogger _logger;

        public LoggingBehavior(ILogger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<TResponse> next)
        {
            _logger.Debug($"Executing {typeof(TRequest).Name}");
            var response = next();
            _logger.Debug($"Executed {typeof(TRequest).Name}.");
            return response;
        }
    }
}
