using System;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using InstaLike.Core.Commands;
using InstaLike.Core.Domain;
using NHibernate;

namespace InstaLike.Web.CommandHandlers
{
    internal sealed class LikeOrDislikePostCommandHandler : ICommandHandler<LikeOrDislikePostCommand>
    {
        private readonly ISession _session;

        public LikeOrDislikePostCommandHandler(ISession session)
        {
            _session = session ?? throw new ArgumentNullException(nameof(session));
        }

        public async Task<Result> HandleAsync(LikeOrDislikePostCommand command)
        {
            using (var tx = _session.BeginTransaction())
            {
                try
                {
                    Post postAlias = null;

                    var user = await _session.LoadAsync<User>(command.UserID);
                    var likeQuery = _session.QueryOver<Like>()
                        .Inner.JoinAlias(l => l.Post, () => postAlias).Fetch()
                        .Where(l => l.Post.ID == command.PostID && l.User == user)
                        .Select(l => l.Post);

                    if (postAlias.LikesTo(user))
                    {
                        postAlias.RemoveLikeBy(user);
                    }
                    else
                    {
                        postAlias.PutLikeFor(user);
                    }

                    await _session.SaveOrUpdateAsync(postAlias);
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
