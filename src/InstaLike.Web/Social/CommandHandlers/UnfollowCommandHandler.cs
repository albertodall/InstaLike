using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
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
        if (unfollowedID is null)
        {
            return Result.Failure($"Member [{request.UnfollowedNickname}] does not exist.");
        }

        var follower = await _session.GetAsync<Follower>(FollowerId.From(request.FollowerID), cancellationToken);
        var result = follower?.Unfollow(unfollowedID.Value) ?? Result.Failure("You are not following this member.");
        if (result.IsFailure)
        {
            _logger.Warning("Member {MemberID} could not unfollow [{UnfollowedNickname}({UnfollowedMemberID})]: {Error}",
                request.FollowerID,
                request.UnfollowedNickname,
                unfollowedID,
                result.Error);

            return result;
        }

        await _session.FlushAsync(cancellationToken);

        _logger.Information("Member {MemberID} stopped following [{UnfollowedNickname}({UnfollowedMemberID})]",
            request.FollowerID,
            request.UnfollowedNickname,
            unfollowedID);

        return Result.Success();
    }
}
