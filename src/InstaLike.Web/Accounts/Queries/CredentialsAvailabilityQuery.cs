using System.Threading;
using System.Threading.Tasks;
using InstaLike.Core.Accounts;
using MediatR;
using NHibernate;
using NHibernate.Criterion;

namespace InstaLike.Web.Accounts.Queries;

/// <summary>
/// Tells whether an account already logs in with the given username or uses the given email address.
/// </summary>
/// <remarks>
/// Runs on the session of the command that asks, so it sees the changes that command has not committed yet.
/// </remarks>
public sealed class CredentialsAvailabilityQuery : IRequest<CredentialsInUse>
{
    public Username Username { get; }
    public Email Email { get; }

    public CredentialsAvailabilityQuery(Username username, Email email)
    {
        Username = username;
        Email = email;
    }
}

public sealed record CredentialsInUse(bool Username, bool Email);

internal sealed class CredentialsAvailabilityQueryHandler : IRequestHandler<CredentialsAvailabilityQuery, CredentialsInUse>
{
    private readonly ISession _session;

    public CredentialsAvailabilityQueryHandler(ISession session)
    {
        _session = session;
    }

    public async Task<CredentialsInUse> Handle(CredentialsAvailabilityQuery request, CancellationToken cancellationToken)
    {
        var accountsWithUsername = _session.QueryOver<Account>()
            .Where(Restrictions.Eq("Username", request.Username.Value))
            .ToRowCountQuery()
            .FutureValue<int>();
        var accountsWithEmail = _session.QueryOver<Account>()
            .Where(Restrictions.Eq("Email", request.Email.Value))
            .ToRowCountQuery()
            .FutureValue<int>();

        return new CredentialsInUse(
            await accountsWithUsername.GetValueAsync(cancellationToken) > 0,
            await accountsWithEmail.GetValueAsync(cancellationToken) > 0);
    }
}
