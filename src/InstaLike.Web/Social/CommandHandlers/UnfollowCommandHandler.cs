using System;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using InstaLike.Core.Accounts;
using InstaLike.Core.Membership;
using InstaLike.Core.Social;
using InstaLike.Web.Social.Commands;
using MediatR;
using NHibernate;
using NHibernate.Criterion;
using Serilog;

namespace InstaLike.Web.Social.CommandHandlers;

internal sealed class UnfollowCommandHandler : IRequestHandler<UnfollowCommand, Result>
{
    private readonly ISession _session;
    private readonly ILogger _logger;

    public UnfollowCommandHandler(ISession session, ILogger logger)
    {
        _session = session;
        _logger = logger.ForContext<UnfollowCommand>();
    }

    public async Task<Result> Handle(UnfollowCommand request, CancellationToken cancellationToken)
    {
        var unfollowedID = await _session.QueryOver<Member>()
            .Where(Restrictions.Eq("Nickname", request.UnfollowedNickname))
            .Select(u => u.ID)
            .SingleOrDefaultAsync<MemberId?>(cancellationToken);
        if (unfollowedID?.Value is not Guid unfollowedUserID)
        {
            return Result.Failure($"User [{request.UnfollowedNickname}] does not exist.");
        }

        var follow = await _session.QueryOver<Follow>()
            .Where(f => f.FollowerID == request.FollowerID && f.FollowedID == unfollowedUserID)
            .SingleOrDefaultAsync(cancellationToken);
        if (follow is null)
        {
            return Result.Failure($"User {request.FollowerID} is not following user [{request.UnfollowedNickname}].");
        }

        try
        {
            await _session.DeleteAsync(follow, cancellationToken);
            await _session.FlushAsync(cancellationToken);
        }
        catch (ADOException ex)
        {
            _logger.Error("Error while unfollowing [{UnfollowedNickname}({UnfollowedUserID})] by user {UserID}. Error message: {ErrorMessage}",
                request.UnfollowedNickname,
                unfollowedUserID,
                request.FollowerID,
                ex.Message);

            return Result.Failure(ex.Message);
        }

        _logger.Information("User {UserID} stopped following user [{UnfollowedNickname}({UnfollowedUserID})]",
            request.FollowerID,
            request.UnfollowedNickname,
            unfollowedUserID);

        return Result.Success();
    }
}
