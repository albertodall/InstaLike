using System;
using System.Threading;
using System.Threading.Tasks;
using InstaLike.Core.Accounts;
using MediatR;
using NHibernate;
using Serilog;

namespace InstaLike.Web.Accounts.EventHandlers;

internal class UserLoggedInEventHandler : INotificationHandler<UserLoggedInEvent>
{
    private readonly ISession _session;
    private readonly ILogger _logger;

    public UserLoggedInEventHandler(ISession session, ILogger logger)
    {
        _session = session ?? throw new ArgumentNullException(nameof(session));
        _logger = logger?.ForContext<UserLoggedInEvent>() ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task Handle(UserLoggedInEvent notification, CancellationToken cancellationToken)
    {
        var user = await _session.GetAsync<User>(notification.UserID, cancellationToken);
        _logger.Information("User [{Nickname}({UserID})] has just logged in.", user?.Nickname, notification.UserID);
    }
}
