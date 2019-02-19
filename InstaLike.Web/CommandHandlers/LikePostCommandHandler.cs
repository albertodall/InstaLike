using System;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using InstaLike.Core.Commands;
using InstaLike.Core.Domain;
using NHibernate;
using NHibernate.Criterion;

namespace InstaLike.Web.CommandHandlers
{
    internal sealed class LikePostCommandHandler : ICommandHandler<LikeOrDislikePostCommand>
    {
        private readonly ISession _session;

        public LikePostCommandHandler(ISession session)
        {
            _session = session ?? throw new ArgumentNullException(nameof(session));
        }

        public async Task<Result> HandleAsync(LikeOrDislikePostCommand command)
        {
            using (var tx = _session.BeginTransaction())
            {
                Post postAlias = null;

                var userQuery = _session.QueryOver<User>()
                        .Where(Restrictions.Eq("Nickname", command.Nickname))
                        .Select(u => u.ID);
                var user = await userQuery.SingleOrDefaultAsync();

                var likeQuery = _session.QueryOver<Like>()
                    .Inner.JoinAlias(l => l.Post, () => postAlias).Fetch()
                    .Where(l => l.Post.ID == command.PostID)
                        .And(Restrictions.Eq("User.Nickname", command.Nickname))
                    .Select(l => l.ID);

                var liked = await likeQuery.SingleOrDefaultAsync();

                if (liked == null)
                {
                    postAlias.Like(user);
                }
                else
                {
                    postAlias.Dislike(user);
                }
                
                await _session.SaveOrUpdateAsync(postAlias);
                await tx.CommitAsync();
                return Result.Ok();
            }
        }
    }
}
