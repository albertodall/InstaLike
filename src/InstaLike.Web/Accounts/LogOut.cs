using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using InstaLike.Core.Accounts;
using InstaLike.Web.Data;
using MediatR;
using NHibernate;
using Serilog;

namespace InstaLike.Web.Accounts
{
    public sealed class LogOutCommand : ICommand<Result>
    {
        public AccountId AccountID { get; }

        public LogOutCommand(AccountId accountID)
        {
            AccountID = accountID;
        }
    }

    internal sealed class LogOutCommandHandler : IRequestHandler<LogOutCommand, Result>
    {
        private readonly ISession _session;

        public LogOutCommandHandler(ISession session)
        {
            _session = session;
        }

        public async Task<Result> Handle(LogOutCommand request, CancellationToken cancellationToken)
        {
            var account = await _session.GetAsync<Account>(request.AccountID, cancellationToken);
            if (account is null)
            {
                return Result.Failure($"Account {request.AccountID} does not exist.");
            }

            account.LogOut();
            return Result.Success();
        }
    }

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
}
