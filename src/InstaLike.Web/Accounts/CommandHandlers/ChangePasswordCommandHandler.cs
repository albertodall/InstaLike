using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using InstaLike.Core.Accounts;
using InstaLike.Web.Accounts.Commands;
using MediatR;
using NHibernate;
using Serilog;

namespace InstaLike.Web.Accounts.CommandHandlers
{
    internal sealed class ChangePasswordCommandHandler : IRequestHandler<ChangePasswordCommand, Result>
    {
        private readonly ISession _session;
        private readonly ILogger _logger;

        public ChangePasswordCommandHandler(ISession session, ILogger logger)
        {
            _session = session;
            _logger = logger.ForContext<ChangePasswordCommand>();
        }

        public async Task<Result> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
        {
            var newPassword = Password.Create(request.NewPassword);
            if (newPassword.IsFailure)
            {
                return newPassword;
            }

            var account = await _session.GetAsync<Account>(request.AccountID, cancellationToken);
            if (account is null)
            {
                return Result.Failure($"Account {request.AccountID} does not exist.");
            }

            var change = account.ChangePassword(request.CurrentPassword, newPassword.Value);
            if (change.IsFailure)
            {
                _logger.Warning("Account [{Username}({AccountID})] could not change its password: {Reason}", account.Username, request.AccountID, change.Error);
                return change;
            }

            await _session.FlushAsync(cancellationToken);

            _logger.Information("Account [{Username}({AccountID})] changed its password.", account.Username, request.AccountID);
            return Result.Success();
        }
    }
}
