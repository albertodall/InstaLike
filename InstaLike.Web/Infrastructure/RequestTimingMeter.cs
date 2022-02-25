using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Serilog;

namespace InstaLike.Web.Infrastructure
{
    internal sealed class RequestTimingMeter<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        private readonly ILogger _logger;
        private readonly Stopwatch _chronometer = new Stopwatch();

        public RequestTimingMeter(ILogger logger)
        {
            _logger = logger?.ForContext<TRequest>() ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<TResponse> next)
        {
            _chronometer.Start();
            _logger.Debug("Begin Request");
            var response = await next();
            _chronometer.Stop();
            _logger.Debug($"End Request. Elapsed time: {_chronometer.ElapsedMilliseconds} ms.");

            return response;
        }
    }
}
