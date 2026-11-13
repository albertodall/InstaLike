using System.Threading;
using System.Threading.Tasks;
using InstaLike.Core.Accounts;
using MediatR;
using NHibernate;
using Serilog;

namespace InstaLike.Web.Accounts.EventHandlers
{
    internal sealed class AccountLoggedInEventHandler : INotificationHandler<AccountLoggedInEvent>
    {
        private readonly ISession _session;
        private readonly ILogger _logger;

        public AccountLoggedInEventHandler(ISession session, ILogger logger)
        {
            _session = session;
            _logger = logger.ForContext<AccountLoggedInEvent>();
        }

        public async Task Handle(AccountLoggedInEvent notification, CancellationToken cancellationToken)
        {
            var account = await _session.GetAsync<Account>(notification.AccountID, cancellationToken);
            _logger.Information("Account [{Username}({AccountID})] has just logged in.", account?.Username, notification.AccountID);
        }
    }
}
