using System;
using System.Linq;
using System.Threading.Tasks;
using InstaLike.Core.Domain;
using InstaLike.Web.Models;
using NHibernate;

namespace InstaLike.Web.Data.Query
{
    public class PostQuery : IQuery<PostModel>
    {
        public int PostID { get; }

        public PostQuery(int postID)
        {
            PostID = postID;
        }
    }

    internal sealed class PostQueryHandler : IQueryHandler<PostQuery, PostModel>
    {
        private readonly ISession _session;

        public PostQueryHandler(ISession session)
        {
            _session = session ?? throw new ArgumentNullException(nameof(session));
        }

        public async Task<PostModel> HandleAsync(PostQuery query)
        {
            Post post = null;
            using (var tx = _session.BeginTransaction())
            {
                _session.QueryOver<Like>()
                    .Fetch(SelectMode.Fetch, l => l.User)
                    .Where(l => l.Post.ID == query.PostID)
                    .Select(l => l.User.Nickname)
                    .Future<string>();

                _session.QueryOver<Comment>()
                    .Fetch(SelectMode.Fetch, c => c.User)
                    .Where(c => c.Post.ID == query.PostID)
                    .Future<Comment>();

                var postQuery = _session.QueryOver<Post>()
                    .Fetch(SelectMode.Fetch, p => p.Author)
                    .Where(p => p.ID == query.PostID)
                    .FutureValue<Post>();

                post = await postQuery.GetValueAsync();
            }

            // AutoMapper?
            return new PostModel()
            {
                PostID = post.ID,
                AuthorNickName = post.Author.Nickname,
                AuthorProfilePicture = post.Author.ProfilePicture,
                Text = post.Text,
                Picture = post.Picture.RawBytes,
                PostDate = post.PostDate,
                UserLikes = post.Likes.Select(l => (string)l.User.Nickname).ToArray(),
                Comments = post.Comments.Select(c => new CommentModel()
                {
                    AuthorNickName = c.User.Nickname,
                    CommentDate = c.CommentDate,
                    Text = c.Text
                }).ToArray()
            };
        }
    }
}
