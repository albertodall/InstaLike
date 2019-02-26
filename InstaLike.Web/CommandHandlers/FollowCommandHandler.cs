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
    internal sealed class FollowCommandHandler : IRequestHandler<FollowCommand, Result>
    {
        private readonly ISession _session;

        public FollowCommandHandler(ISession session)
        {
            _session = session ?? throw new ArgumentNullException(nameof(session));
        }

        public async Task<Result> Handle(FollowCommand command, CancellationToken cancellationToken)
        {
            using (var tx = _session.BeginTransaction())
            {
                try
                {
                    var follower = await _session.LoadAsync<User>(command.FollowerID);
                    var followedUserQuery = _session.QueryOver<User>()
                        .Where(Restrictions.Eq("Nickname", command.FollowedNickname));

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
