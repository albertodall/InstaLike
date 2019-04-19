using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using InstaLike.Core.Domain;
using InstaLike.Web.Models;
using MediatR;
using NHibernate;
using NHibernate.Transform;
using Serilog;

namespace InstaLike.Web.Data.Query
{
    public class PostCommentsQuery : IRequest<CommentModel[]>
    {
        public int PostID { get;  }

        public PostCommentsQuery(int postID)
        {
            PostID = postID;
        }
    }

    internal class PostCommentsQueryHandler : IRequestHandler<PostCommentsQuery, CommentModel[]>
    {
        private readonly ISession _session;
        private readonly ILogger _logger;

        public PostCommentsQueryHandler(ISession session, ILogger logger)
        {
            _session = session ?? throw new ArgumentNullException(nameof(session));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<CommentModel[]> Handle(PostCommentsQuery request, CancellationToken cancellationToken)
        {
            CommentModel[] result = null;

            _logger.Debug("Reading comments list for post {PostID} with parameters {@Request}", request.PostID, request);

            using (var tx = _session.BeginTransaction())
            {
                CommentModel comment = null;
                User commentAuthor = null;
                var commentsQuery = _session.QueryOver<Comment>()
                    .Where(c => c.Post.ID == request.PostID)
                    .OrderBy(c => c.CommentDate).Asc
                    .Inner.JoinQueryOver(c => c.Author, () => commentAuthor)
                    .SelectList(list => list
                        .Select(() => commentAuthor.Nickname).WithAlias(() => comment.AuthorNickName)
                        .Select(c => c.Text).WithAlias(() => comment.Text)
                        .Select(c => c.CommentDate).WithAlias(() => comment.CommentDate)
                    )
                    .TransformUsing(Transformers.AliasToBean<CommentModel>());

                result = (await commentsQuery.ListAsync<CommentModel>()).ToArray();
            }

            return result;
        }
    }
}
