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
    internal sealed class LikeOrDislikePostCommandHandler : IRequestHandler<LikeOrDislikePostCommand, Result<LikePostResult>>
    {
        private readonly ISession _session;

        public LikeOrDislikePostCommandHandler(ISession session)
        {
            _session = session ?? throw new ArgumentNullException(nameof(session));
        }

        public async Task<Result<LikePostResult>> Handle(LikeOrDislikePostCommand request, CancellationToken cancellationToken)
        {
            using (var tx = _session.BeginTransaction())
            {
                try
                {
                    Like likeAlias = null;

                    var user = await _session.LoadAsync<User>(request.UserID);
                    var postQuery = _session.QueryOver<Post>()
                        .Left.JoinAlias(p => p.Likes, () => likeAlias)
                        .Where(p => p.ID == request.PostID)
                        .And(
                            Restrictions.Disjunction()
                                .Add(() => likeAlias.User.ID == request.UserID)
                                .Add(Restrictions.On(() => likeAlias.User).IsNull)
                            );

                    var post = await postQuery.SingleOrDefaultAsync();
                    var likeResult = post.Like(user);

                    await _session.SaveOrUpdateAsync(post);
                    await tx.CommitAsync();
                    return Result.Ok(likeResult);
                }
                catch (ADOException ex)
                {
                    await tx.RollbackAsync();
                    return Result.Fail<LikePostResult>(ex.Message);
                }
            }
        }
    }
}
