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
    public class PostDetailQuery : IRequest<PostModel>
    {
        public int PostID { get; }
        public int CurrentUserID { get; }

        public PostDetailQuery(int postID, int currentUserID)
        {
            PostID = postID;
            CurrentUserID = currentUserID;
        }
    }

    internal sealed class PostDetailQueryHandler : IRequestHandler<PostDetailQuery, PostModel>
    {
        private readonly ISession _session;
        private readonly ILogger _logger;

        public PostDetailQueryHandler(ISession session, ILogger logger)
        {
            _session = session ?? throw new ArgumentNullException(nameof(session));
            _logger = logger?.ForContext<PostDetailQuery>() ?? throw new ArgumentNullException(nameof(logger)) ;
        }

        public async Task<PostModel> Handle(PostDetailQuery request, CancellationToken cancellationToken)
        {
            PostModel post = null;

            _logger.Debug("Reading details for post {PostID} with parameters {@Request}", request.PostID, request);

            using (var tx = _session.BeginTransaction())
            {
                // Total likes for this post
                var postLikesCountQuery =_session.QueryOver<Like>()
                    .Where(l => l.Post.ID == request.PostID)
                    .ToRowCountQuery()
                    .FutureValue<int>();

                // Comments for this post
                User commentAuthor = null;
                CommentModel comment = null;
                var postCommentsQuery = _session.QueryOver<Comment>()
                    .Where(c => c.Post.ID == request.PostID)
                    .OrderBy(c => c.CommentDate).Asc
                    .Inner.JoinQueryOver(c => c.Author, () => commentAuthor)
                    .SelectList(list => list
                        .Select(c => c.Text).WithAlias(() => comment.Text)
                        .Select(c => c.CommentDate).WithAlias(() => comment.CommentDate)
                        .Select(() => commentAuthor.Nickname).WithAlias(() => comment.AuthorNickName)
                    )
                    .TransformUsing(Transformers.AliasToBean<CommentModel>())
                    .Future<CommentModel>();

                // Is this post liked by the current user?
                var isLikedByCurrentUserQuery = _session.QueryOver<Like>()
                    .Where(l => l.Post.ID == request.PostID)
                        .And(l => l.User.ID == request.CurrentUserID)
                    .ToRowCountQuery()
                    .FutureValue<int>();

                // Post to show
                User postAuthor = null;
                var postQuery = _session.QueryOver<Post>()
                    .Where(p => p.ID == request.PostID)
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

                // Add related data
                post = await postQuery.GetValueAsync();
                post.LikesCount = await postLikesCountQuery.GetValueAsync();
                post.Comments = postCommentsQuery.ToArray();
                post.IsLikedByCurrentUser = (await isLikedByCurrentUserQuery.GetValueAsync()) > 0;
            }

            return post;
        }
    }
}
