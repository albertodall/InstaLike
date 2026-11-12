using System.Threading;
using System.Threading.Tasks;
using InstaLike.Core.Accounts;
using MediatR;
using NHibernate;
using Serilog;

namespace InstaLike.Web.Accounts.EventHandlers;

internal sealed class UserLoggedOutEventHandler : INotificationHandler<UserLoggedOutEvent>
{
    private readonly ISession _session;
    private readonly ILogger _logger;

    public UserLoggedOutEventHandler(ISession session, ILogger logger)
    {
        _session = session;
        _logger = logger.ForContext<UserLoggedOutEvent>();
    }

    public async Task Handle(UserLoggedOutEvent notification, CancellationToken cancellationToken)
    {
        var user = await _session.GetAsync<User>(notification.UserID, cancellationToken);
        _logger.Information("User [{Nickname}({UserID})] has just logged out.", user?.Nickname, notification.UserID);
    }
}
