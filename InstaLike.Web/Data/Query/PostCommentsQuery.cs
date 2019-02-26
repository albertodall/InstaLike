using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using InstaLike.Core.Domain;
using InstaLike.Web.Models;
using MediatR;
using NHibernate;
using NHibernate.Transform;

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

        public PostCommentsQueryHandler(ISession session)
        {
            _session = session ?? throw new ArgumentNullException(nameof(session));
        }

        public async Task<CommentModel[]> Handle(PostCommentsQuery query, CancellationToken cancellationToken)
        {
            CommentModel[] result = null;

            using (var tx = _session.BeginTransaction())
            {
                CommentModel comment = null;
                User commentAuthor = null;
                var commentsQuery = _session.QueryOver<Comment>()
                    .Where(c => c.Post.ID == query.PostID)
                    .OrderBy(c => c.CommentDate).Desc
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
