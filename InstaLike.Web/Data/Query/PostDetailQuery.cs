using System;
using System.Linq;
using System.Threading.Tasks;
using InstaLike.Core.Domain;
using InstaLike.Web.Models;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Transform;

namespace InstaLike.Web.Data.Query
{
    public class PostDetailQuery : IQuery<PostModel>
    {
        public int PostID { get; }

        public PostDetailQuery(int postID)
        {
            PostID = postID;
        }
    }

    internal sealed class PostDetailQueryHandler : IQueryHandler<PostDetailQuery, PostModel>
    {
        private readonly ISession _session;

        public PostDetailQueryHandler(ISession session)
        {
            _session = session ?? throw new ArgumentNullException(nameof(session));
        }

        public async Task<PostModel> HandleAsync(PostDetailQuery query)
        {
            PostModel post = null;

            using (var tx = _session.BeginTransaction())
            {
                User userWhoLiked = null;
                var postLikesQuery =_session.QueryOver<Like>()
                    .Where(l => l.Post.ID == query.PostID)
                    .Inner.JoinQueryOver(l => l.User, () => userWhoLiked)
                    .Select(Projections.Property(() => userWhoLiked.Nickname))
                    .Future<string>();

                User commentAuthor = null;
                CommentModel comment = null;
                var postCommentsQuery = _session.QueryOver<Comment>()
                    .Where(c => c.Post.ID == query.PostID)
                    .Inner.JoinQueryOver(c => c.User, () => commentAuthor)
                    .SelectList(list => list
                        .Select(c => c.Text).WithAlias(() => comment.Text)
                        .Select(c => c.CommentDate).WithAlias(() => comment.CommentDate)
                        .Select(() => commentAuthor.Nickname).WithAlias(() => comment.AuthorNickName)
                    )
                    .TransformUsing(Transformers.AliasToBean<CommentModel>())
                    .Future<CommentModel>();

                User postAuthor = null;
                var postQuery = _session.QueryOver<Post>()
                    .Where(p => p.ID == query.PostID)
                    .Inner.JoinQueryOver(p => p.Author, () => postAuthor)
                    .SelectList(list => list
                        .Select(p => p.ID).WithAlias(() => post.PostID)
                        .Select(p => p.Text).WithAlias(() => post.Text)
                        .Select(p => p.Picture.RawBytes).WithAlias(() => post.Picture)
                        .Select(p => p.PostDate).WithAlias(() => post.PostDate)
                        .Select(() => postAuthor.Nickname).WithAlias(() => post.AuthorNickName)
                        .Select(() => postAuthor.ProfilePicture.RawBytes).WithAlias(() => post.AuthorProfilePicture)                        
                    )
                    .TransformUsing(Transformers.AliasToBean<PostModel>())
                    .FutureValue<PostModel>();

                post = await postQuery.GetValueAsync();
                post.UserLikes = (await postLikesQuery.GetEnumerableAsync()).ToArray();
                post.Comments = (await postCommentsQuery.GetEnumerableAsync()).ToArray();     
            }

            return post;
        }
    }
}
