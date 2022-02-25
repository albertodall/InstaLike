using System;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using InstaLike.Core.Commands;
using InstaLike.Core.Domain;
using MediatR;
using NHibernate;
using NHibernate.Criterion;
using Serilog;

#nullable disable

namespace InstaLike.Web.CommandHandlers
{
    internal sealed class UnfollowCommandHandler : IRequestHandler<UnfollowCommand, Result>
    {
        private readonly ISession _session;
        private readonly ILogger _logger;

        public UnfollowCommandHandler(ISession session, ILogger logger)
        {
            _session = session ?? throw new ArgumentNullException(nameof(session));
            _logger = logger?.ForContext<UnfollowCommand>() ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Result> Handle(UnfollowCommand request, CancellationToken cancellationToken)
        {
            using (var tx = _session.BeginTransaction())
            {
                Follow follow = null;

                var userToUnfollow = await _session.QueryOver<User>()
                    .Where(Restrictions.Eq("Nickname", request.UnfollowedNickname))
                    .SingleOrDefaultAsync(cancellationToken);

                var followingQuery = _session.QueryOver<User>()
                    .Where(u => u.ID == request.FollowerID)
                    .Left.JoinAlias(u => u.Followed, () => follow)
                    .Where(Restrictions.Disjunction()
                        .Add(() => follow.Followed == userToUnfollow)
                        .Add(Restrictions.On(() => follow.Followed).IsNull)
                    );

                var followerUser = await followingQuery.SingleOrDefaultAsync(cancellationToken);
                return await followerUser.Unfollow(userToUnfollow)
                    .OnSuccessTry(async () => await tx.CommitAsync(cancellationToken))
                        .Tap(() => _logger.Information("User {UserID} stopped following user [{UnfollowedNickname}({UnfollowedUserID})]",
                            request.FollowerID,
                            request.UnfollowedNickname,
                            userToUnfollow.ID))
                        .OnFailure(async errorMessage =>
                        {
                            await tx.RollbackAsync(cancellationToken);

                            _logger.Error("Error while unfollowing [{FollowedUserNickname}({FollowedUserID})] by user {UserID}. Error message {ErrorMessage}",
                                request.UnfollowedNickname,
                                userToUnfollow?.ID,
                                request.FollowerID);
                        });
            }
        }
    }
}
