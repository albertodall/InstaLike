using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using InstaLike.Core.Domain;
using InstaLike.Web.Models;
using MediatR;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Transform;

namespace InstaLike.Web.Data.Query
{
    public class PostDetailQuery : IRequest<PostModel>
    {
        public int PostID { get; }
        public int CurrendUserID { get; }

        public PostDetailQuery(int postID, int currentUserID)
        {
            PostID = postID;
            CurrendUserID = currentUserID;
        }
    }

    internal sealed class PostDetailQueryHandler : IRequestHandler<PostDetailQuery, PostModel>
    {
        private readonly ISession _session;

        public PostDetailQueryHandler(ISession session)
        {
            _session = session ?? throw new ArgumentNullException(nameof(session));
        }

        public async Task<PostModel> Handle(PostDetailQuery query, CancellationToken cancellationToken)
        {
            PostModel post = null;

            using (var tx = _session.BeginTransaction())
            {
                var postLikesCountQuery =_session.QueryOver<Like>()
                    .Where(l => l.Post.ID == query.PostID)
                    .ToRowCountQuery()
                    .FutureValue<int>();

                User commentAuthor = null;
                CommentModel comment = null;
                var postCommentsQuery = _session.QueryOver<Comment>()
                    .Where(c => c.Post.ID == query.PostID)
                    .Inner.JoinQueryOver(c => c.Author, () => commentAuthor)
                    .SelectList(list => list
                        .Select(c => c.Text).WithAlias(() => comment.Text)
                        .Select(c => c.CommentDate).WithAlias(() => comment.CommentDate)
                        .Select(() => commentAuthor.Nickname).WithAlias(() => comment.AuthorNickName)
                    )
                    .TransformUsing(Transformers.AliasToBean<CommentModel>())
                    .Future<CommentModel>();

                var isLikedByCurrentUserQuery = _session.QueryOver<Like>()
                    .Where(p => p.ID == query.PostID).And(l => l.User.ID == query.CurrendUserID)
                    .ToRowCountQuery()
                    .FutureValue<byte>();

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
                post.LikesCount = await postLikesCountQuery.GetValueAsync();
                post.Comments = (await postCommentsQuery.GetEnumerableAsync()).ToArray();
                post.IsLikedByCurrentUser = await isLikedByCurrentUserQuery.GetValueAsync();
            }

            return post;
        }
    }
}
