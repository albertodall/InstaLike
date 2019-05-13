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
                    var followerUser = await _session.LoadAsync<User>(request.FollowerID);
                    userToUnfollow = await _session.QueryOver<User>()
                        .Where(Restrictions.Eq("Nickname", request.UnfollowedNickname))
                        .SingleOrDefaultAsync();

                    await _session.QueryOver<Follow>()
                        .Where(f => f.Follower.ID == request.FollowerID).And(f => f.Followed == userToUnfollow)
                        .SingleOrDefaultAsync();

                    return await followerUser.Unfollow(userToUnfollow)
                        .OnSuccess(async () => 
                        {
                            await tx.CommitAsync();

                            _logger.Information("User {UserID} stopped following user [{UnfollowedNickname}({UnfollowedUserID})]",
                                request.FollowerID,
                                request.UnfollowedNickname,
                                userToUnfollow.ID);
                        });
                }
                catch (ADOException ex)
                {
                    await tx.RollbackAsync();
                    
                    _logger.Error("Error while unfollowing [{FollowedUserNickname}({FollowedUserID})] by user {UserID}. Error message {ErrorMessage}",
                        request.UnfollowedNickname,
                        userToUnfollow?.ID,
                        request.FollowerID,
                        ex.Message);

                    return Result.Fail(ex.Message);
                }
            }
        }
    }
}
