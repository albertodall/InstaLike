using System;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using InstaLike.Core.Commands;
using InstaLike.Core.Domain;
using NHibernate;
using NHibernate.Criterion;

namespace InstaLike.Web.CommandHandlers
{
    internal sealed class FollowCommandHandler : ICommandHandler<FollowCommand>
    {
        private readonly ISession _session;

        public FollowCommandHandler(ISession session)
        {
            _session = session ?? throw new ArgumentNullException(nameof(session));
        }

        public async Task<Result> HandleAsync(FollowCommand command)
        {
            using (var tx = _session.BeginTransaction())
            {
                try
                {
                    var follower = await _session.LoadAsync<User>(command.FollowerID);
                    var followedUserQuery = _session.QueryOver<User>()
                        .Where(Restrictions.Eq("Nickname", command.FollowedNickname))
                        .Select(u => u.ID);

                    var followedUser = await followedUserQuery.SingleOrDefaultAsync();

                    follower.Follow(followedUser);
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
