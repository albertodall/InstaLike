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
                    var user = await _session.LoadAsync<User>(command.UserID);
                    var likeQuery = _session.QueryOver<Like>()
                        .Fetch(SelectMode.Fetch, l => l.Post)
                        .Where(l => l.Post.ID == command.PostID && l.User == user)
                        .Select(l => l.Post);
                        
                    var post = await likeQuery.SingleOrDefaultAsync<Post>();

                    if (post.LikesTo(user))
                    {
                        post.RemoveLikeBy(user);
                    }
                    else
                    {
                        post.PutLikeFor(user);
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
