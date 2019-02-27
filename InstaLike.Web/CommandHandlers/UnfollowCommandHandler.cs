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

        public async Task<Result> Handle(UnfollowCommand command, CancellationToken cancellationToken)
        {
            using (var tx = _session.BeginTransaction())
            {
                try
                {
                    var follower = await _session.LoadAsync<User>(command.FollowerID);
                    var unfollowedUserQuery = _session.QueryOver<User>()
                        .Where(Restrictions.Eq("Nickname", command.UnfollowedNickname));

                    var unfollowedUser = await unfollowedUserQuery.SingleOrDefaultAsync();

                    follower.Unfollow(unfollowedUser);
                    await _session.SaveOrUpdateAsync(follower);
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
