using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using InstaLike.Core.Accounts;
using InstaLike.Web.Accounts.Commands;
using MediatR;
using NHibernate;
using NHibernate.Criterion;
using Serilog;

namespace InstaLike.Web.Accounts.CommandHandlers
{
    internal sealed class ChangeEmailCommandHandler : IRequestHandler<ChangeEmailCommand, Result>
    {
        private const string EmailInUse = "This email is already used by another account.";

        private readonly ISession _session;
        private readonly ILogger _logger;

        public ChangeEmailCommandHandler(ISession session, ILogger logger)
        {
            _session = session;
            _logger = logger.ForContext<ChangeEmailCommand>();
        }

        public async Task<Result> Handle(ChangeEmailCommand request, CancellationToken cancellationToken)
        {
            var email = Email.Create(request.Email);
            if (email.IsFailure)
            {
                return email;
            }

            var account = await _session.GetAsync<Account>(request.AccountID, cancellationToken);
            if (account is null)
            {
                return Result.Failure($"Account {request.AccountID} does not exist.");
            }

            var availability = await CheckEmailAvailabilityAsync(request.AccountID, email.Value, cancellationToken);
            if (availability.IsFailure)
            {
                return availability;
            }

            account.ChangeEmailAddress(email.Value);
            await _session.FlushAsync(cancellationToken);

            _logger.Information("Account [{Username}({AccountID})] changed its email address.", account.Username, request.AccountID);
            return Result.Success();
        }

        private async Task<Result> CheckEmailAvailabilityAsync(AccountId accountID, Email email, CancellationToken cancellationToken)
        {
            var otherAccountsWithEmail = await _session.QueryOver<Account>()
                .Where(Restrictions.Eq("Email", email.Value))
                .And(a => a.ID != accountID)
                .RowCountAsync(cancellationToken);

            return Result.FailureIf(otherAccountsWithEmail > 0, EmailInUse);
        }
    }
}
