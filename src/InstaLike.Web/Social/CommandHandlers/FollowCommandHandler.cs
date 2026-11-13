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

internal sealed class FollowCommandHandler : IRequestHandler<FollowCommand, Result>
{
    private readonly ISession _session;
    private readonly ILogger _logger;

    public FollowCommandHandler(ISession session, ILogger logger)
    {
        _session = session;
        _logger = logger.ForContext<FollowCommand>();
    }

    public async Task<Result> Handle(FollowCommand request, CancellationToken cancellationToken)
    {
        var followedID = await _session.QueryOver<Member>()
            .Where(Restrictions.Eq("Nickname", request.FollowedNickname))
            .Select(u => u.ID)
            .SingleOrDefaultAsync<MemberId?>(cancellationToken);
        if (followedID is null)
        {
            return Result.Failure($"Member [{request.FollowedNickname}] does not exist.");
        }

        var follower = await _session.GetAsync<Follower>(FollowerId.From(request.FollowerID), cancellationToken);
        var isFirstFollow = follower is null;
        follower ??= new Follower(request.FollowerID);

        var result = follower.Follow(followedID.Value);
        if (result.IsFailure)
        {
            _logger.Warning("Member {MemberID} could not follow [{FollowedNickname}({FollowedMemberID})]: {Error}",
                request.FollowerID,
                request.FollowedNickname,
                followedID,
                result.Error);

            return result;
        }

        if (isFirstFollow)
        {
            await _session.SaveAsync(follower, cancellationToken);
        }

        await _session.FlushAsync(cancellationToken);

        _logger.Information("Member {MemberID} started following [{FollowedNickname}({FollowedMemberID})]",
            request.FollowerID,
            request.FollowedNickname,
            followedID);

        return Result.Success();
    }
}
