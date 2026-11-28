using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using InstaLike.Core.Membership;
using InstaLike.Core.Posts;
using InstaLike.Core.SharedKernel;
using InstaLike.Web.Data;
using MediatR;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.SqlCommand;
using Serilog;

namespace InstaLike.Web.Posts
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
                Comment commentEntity = null!;
                Member commentAuthor = null!;
                var commentsQuery = _session.QueryOver(() => commentEntity)
                    .JoinEntityAlias(() => commentAuthor, Restrictions.EqProperty(Projections.Property(() => commentAuthor.ID), Projections.Property(() => commentEntity.AuthorID)), JoinType.InnerJoin)
                    .Where(() => commentEntity.Post.ID == request.PostID)
                    .OrderBy(() => commentEntity.CommentDate).Asc
                    .SelectInto(() => new CommentModel
                    {
                        AuthorNickName = commentAuthor.Nickname,
                        Text = commentEntity.Text,
                        CommentDate = commentEntity.CommentDate
                    });

                result = (await commentsQuery.ListAsync<CommentModel>(cancellationToken)).ToArray();
            }

            return result;
        }
    }
}
