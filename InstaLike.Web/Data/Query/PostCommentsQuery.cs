using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InstaLike.Core.Domain;
using InstaLike.Web.Models;
using NHibernate;

namespace InstaLike.Web.Data.Query
{
    public class PostCommentsQuery : IQuery<CommentModel[]>
    {
        public int PostID { get;  }

        public PostCommentsQuery(int postID)
        {
            PostID = postID;
        }
    }

    internal class PostCommentsQueryHandler : IQueryHandler<PostCommentsQuery, CommentModel[]>
    {
        private readonly ISession _session;

        public PostCommentsQueryHandler(ISession session)
        {
            _session = session ?? throw new ArgumentNullException(nameof(session));
        }

        public async Task<CommentModel[]> HandleAsync(PostCommentsQuery query)
        {
            IList<Comment> result = null;
            using (var tx = _session.BeginTransaction())
            {
                var commentsQuery = _session.QueryOver<Comment>()
                    .Fetch(SelectMode.Fetch, c => c.User)
                    .Where(c => c.Post.ID == query.PostID)
                    .OrderBy(c => c.CommentDate).Desc;

                result = await commentsQuery.ListAsync();
            }

            // Automapper?
            return result.Select(c => new CommentModel()
            {
                AuthorNickName = c.User.Nickname,
                CommentDate = c.CommentDate,
                Text = c.Text
            }).ToArray();
        }
    }
}
