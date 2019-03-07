using System;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using InstaLike.Core.Commands;
using InstaLike.Core.Domain;
using MediatR;
using NHibernate;
using NHibernate.Criterion;

namespace InstaLike.Web.CommandHandlers
{
    internal sealed class UnfollowCommandHandler : IRequestHandler<UnfollowCommand, Result>
    {
        private readonly ISession _session;

        public UnfollowCommandHandler(ISession session)
        {
            _session = session ?? throw new ArgumentNullException(nameof(session));
        }

        public async Task<Result> Handle(UnfollowCommand request, CancellationToken cancellationToken)
        {
            using (var tx = _session.BeginTransaction())
            {
                try
                {
                    User following = null;

                    var userToUnfollow = await _session.QueryOver<User>()
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
                    return Result.Ok();
                }
                catch (ADOException ex)
                {
                    await tx.RollbackAsync();
                    return Result.Fail(ex.Message);
                }
            }
        }
    }
}
