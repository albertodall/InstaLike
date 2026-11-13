using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using InstaLike.Core.Accounts;
using InstaLike.Core.Membership;
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
        var member = await _session.GetAsync<Member>(request.MemberID, cancellationToken);
        if (member is null)
        {
            return Result.Failure($"Member {request.MemberID} does not exist.");
        }

        member.LogOut();
        return Result.Success();
    }
}
