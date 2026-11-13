using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using InstaLike.Core.Accounts;
using InstaLike.Web.Accounts.Commands;
using MediatR;
using NHibernate;

namespace InstaLike.Web.Accounts.CommandHandlers;

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
