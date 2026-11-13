using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using InstaLike.Core.Accounts;
using InstaLike.Core.Membership;
using InstaLike.Core.Posts;
using InstaLike.Web.Models;
using MediatR;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.SqlCommand;
using NHibernate.Transform;
using Serilog;

namespace InstaLike.Web.Posts.Queries
{
    public sealed class PostCommentsQuery : IRequest<CommentModel[]>
    {
        public PostId PostID { get; }

        public PostCommentsQuery(PostId postID)
        {
            PostID = postID;
        }
    }

    internal sealed class PostCommentsQueryHandler : IRequestHandler<PostCommentsQuery, CommentModel[]>
    {
        private readonly IStatelessSession _session;
        private readonly ILogger _logger;

        public PostCommentsQueryHandler(IStatelessSession session, ILogger logger)
        {
            _session = session;
            _logger = logger;
        }

        public async Task<CommentModel[]> Handle(PostCommentsQuery request, CancellationToken cancellationToken)
        {
            CommentModel[] result;

            _logger.Debug("Reading comments list for post {PostID} with parameters {@Request}", request.PostID, request);

            using (var tx = _session.BeginTransaction())
            {
                CommentModel comment = null!;
                Comment commentEntity = null!;
                Member commentAuthor = null!;
                var commentsQuery = _session.QueryOver(() => commentEntity)
                    .JoinEntityAlias(() => commentAuthor, Restrictions.EqProperty(Projections.Property(() => commentAuthor.ID), Projections.Property(() => commentEntity.AuthorID)), JoinType.InnerJoin)
                    .Where(() => commentEntity.Post.ID == request.PostID)
                    .OrderBy(() => commentEntity.CommentDate).Asc
                    .SelectList(list => list
                        .Select(() => commentAuthor.Nickname).WithAlias(() => comment.AuthorNickName)
                        .Select(() => commentEntity.Text).WithAlias(() => comment.Text)
                        .Select(() => commentEntity.CommentDate).WithAlias(() => comment.CommentDate)
                    )
                    .TransformUsing(Transformers.AliasToBean<CommentModel>());

                result = (await commentsQuery.ListAsync<CommentModel>(cancellationToken)).ToArray();
            }

            return result;
        }
    }
}
