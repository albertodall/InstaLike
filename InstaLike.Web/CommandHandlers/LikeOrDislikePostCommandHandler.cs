using System;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using InstaLike.Core.Commands;
using InstaLike.Core.Domain;
using MediatR;
using NHibernate;

namespace InstaLike.Web.CommandHandlers
{
    internal sealed class LikeOrDislikePostCommandHandler : IRequestHandler<LikeOrDislikePostCommand, Result>
    {
        private readonly ISession _session;

        public LikeOrDislikePostCommandHandler(ISession session)
        {
            _session = session ?? throw new ArgumentNullException(nameof(session));
        }

        public async Task<Result> Handle(LikeOrDislikePostCommand command, CancellationToken cancellationToken)
        {
            using (var tx = _session.BeginTransaction())
            {
                try
                {
                    Like like = null;

                    var user = await _session.LoadAsync<User>(command.UserID);
                    var post = await _session.LoadAsync<Post>(command.PostID);
                    var likeQuery = _session.QueryOver(() => like)
                        .Where(() => like.Post.ID == command.PostID && like.User.ID == command.UserID);

                    var postLike = await likeQuery.SingleOrDefaultAsync();
                    if (postLike == null)
                    {
                        post.PutLikeFor(user);
                    }
                    else
                    {
                        post.RemoveLike(postLike);
                    }

                    await _session.SaveOrUpdateAsync(post);
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
