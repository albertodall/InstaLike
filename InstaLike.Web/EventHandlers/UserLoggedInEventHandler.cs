using System;
using System.Threading;
using System.Threading.Tasks;
using InstaLike.Core.Events;
using MediatR;
using Serilog;

namespace InstaLike.Web.EventHandlers
{
    internal class UserLoggedInEventHandler : INotificationHandler<UserLoggedInEvent>
    {
        private readonly ILogger _logger;

        public UserLoggedInEventHandler(ILogger logger)
        {
            _logger = logger?.ForContext<UserLoggedInEvent>() ?? throw new ArgumentNullException(nameof(logger));
        }

        public Task Handle(UserLoggedInEvent notification, CancellationToken cancellationToken)
        {
            _logger.Information("User {UserID} ({Nickname}) has just logged in.", notification.UserID, notification.Nickname);
            return Task.CompletedTask;
        }
    }
}
