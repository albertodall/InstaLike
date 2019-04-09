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
                User userToUnfollow = null;
                try
                {
                    User following = null;

                    userToUnfollow = await _session.QueryOver<User>()
                        .Where(Restrictions.Eq("Nickname", request.UnfollowedNickname))
                        .SingleOrDefaultAsync();

                    var followingQuery = _session.QueryOver<User>()
                        .Where(u => u.ID == request.FollowerID)
                        .Left.JoinAlias(u => u.Following, () => following)
                        .Where(() => following.ID == userToUnfollow.ID);

                    var followerUser = await followingQuery.SingleOrDefaultAsync();

                    followerUser.Unfollow(userToUnfollow);
                    await _session.SaveOrUpdateAsync(followerUser);
                    await tx.CommitAsync();

                    _logger.Information("User {UserID} stopped following user [{UnfollowedNickname}({UnfollowedUserID})]",
                        request.FollowerID,
                        userToUnfollow.ID,
                        request.UnfollowedNickname);

                    return Result.Ok();
                }
                catch (ADOException ex)
                {
                    await tx.RollbackAsync();
                    
                    _logger.Error("Error while unfollowing [{FollowedUserNickname}({FollowedUserID})] by user {UserID}. Error message {ErrorMessage}",
                        request.UnfollowedNickname,
                        userToUnfollow?.ID,
                        request.FollowerID);

                    return Result.Fail(ex.Message);
                }
            }
        }
    }
}
