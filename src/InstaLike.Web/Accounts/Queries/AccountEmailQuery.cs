using System.Threading;
using System.Threading.Tasks;
using InstaLike.Core.Accounts;
using MediatR;
using NHibernate;

namespace InstaLike.Web.Accounts.Queries
{
    public sealed class AccountEmailQuery : IRequest<string>
    {
        public AccountId AccountID { get; }

        public AccountEmailQuery(AccountId accountID)
        {
            AccountID = accountID;
        }
    }

    internal sealed class AccountEmailQueryHandler : IRequestHandler<AccountEmailQuery, string>
    {
        private readonly IStatelessSession _session;

        public AccountEmailQueryHandler(IStatelessSession session)
        {
            _session = session;
        }

        public async Task<string> Handle(AccountEmailQuery request, CancellationToken cancellationToken)
        {
            using var tx = _session.BeginTransaction();

            var email = await _session.QueryOver<Account>()
                .Where(a => a.ID == request.AccountID)
                .Select(a => a.Email)
                .SingleOrDefaultAsync<string>(cancellationToken);

            await tx.CommitAsync(cancellationToken);
            return email;
        }
    }
}
