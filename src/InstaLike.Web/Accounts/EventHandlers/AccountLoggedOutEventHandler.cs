using System.Threading;
using System.Threading.Tasks;
using InstaLike.Core.Accounts;
using MediatR;
using NHibernate;
using Serilog;

namespace InstaLike.Web.Accounts.EventHandlers;

internal sealed class AccountLoggedOutEventHandler : INotificationHandler<AccountLoggedOutEvent>
{
    private readonly ISession _session;
    private readonly ILogger _logger;

    public AccountLoggedOutEventHandler(ISession session, ILogger logger)
    {
        _session = session;
        _logger = logger.ForContext<AccountLoggedOutEvent>();
    }

    public async Task Handle(AccountLoggedOutEvent notification, CancellationToken cancellationToken)
    {
        var account = await _session.GetAsync<Account>(notification.AccountID, cancellationToken);
        _logger.Information("Account [{Username}({AccountID})] has just logged out.", account?.Username, notification.AccountID);
    }
}
