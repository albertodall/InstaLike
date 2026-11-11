using System;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using InstaLike.Core.Accounts;
using MediatR;
using NHibernate;

namespace InstaLike.Web.Accounts.CommandHandlers;

internal sealed class LogOutCommandHandler : IRequestHandler<LogOutCommand, Result>
{
    private readonly ISession _session;

    public LogOutCommandHandler(ISession session)
    {
        _session = session ?? throw new ArgumentNullException(nameof(session));
    }

    public async Task<Result> Handle(LogOutCommand request, CancellationToken cancellationToken)
    {
        var user = await _session.GetAsync<User>(request.UserID, cancellationToken);
        if (user is null)
        {
            return Result.Failure($"User {request.UserID} does not exist.");
        }

        user.LogOut();
        return Result.Success();
    }
}
