using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using InstaLike.Core.Accounts;
using InstaLike.Core.Posts;
using InstaLike.Web.Data;
using InstaLike.Web.Models;
using MediatR;
using NHibernate;
using NHibernate.Transform;
using Serilog;

namespace InstaLike.Web.Posts.Queries;

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
        _logger = logger?.ForContext<PostDetailQuery>() ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<PostModel> Handle(PostDetailQuery request, CancellationToken cancellationToken)
    {
        PostModel post = null!;

        _logger.Debug("Reading details for post {PostID} with parameters {@Request}", request.PostID, request);

        using (var tx = _session.BeginTransaction())
        {
            // Total likes for this post
            var postLikesCountQuery = _session.QueryOver<Like>()
                .Where(l => l.Post.ID == request.PostID)
                .ToRowCountQuery()
                .FutureValue<int>();

            // Comments for this post
            Comment commentEntity = null!;
            User commentAuthor = null!;
            CommentModel comment = null!;
            var postCommentsQuery = _session.QueryOver(() => commentEntity)
                .JoinEntityAlias(() => commentAuthor, () => commentAuthor.ID == commentEntity.AuthorID)
                .Where(() => commentEntity.PostID == request.PostID)
                .OrderBy(() => commentEntity.CommentDate).Asc
                .SelectList(list => list
                    .Select(() => commentEntity.Text).WithAlias(() => comment.Text)
                    .Select(() => commentEntity.CommentDate).WithAlias(() => comment.CommentDate)
                    .Select(() => commentAuthor.Nickname).WithAlias(() => comment.AuthorNickName)
                )
                .TransformUsing(Transformers.AliasToBean<CommentModel>())
                .Future<CommentModel>();

            // Is this post liked by the current user?
            var isLikedByCurrentUserQuery = _session.QueryOver<Like>()
                .Where(l => l.Post.ID == request.PostID)
                    .And(l => l.UserID == request.CurrentUserID)
                .ToRowCountQuery()
                .FutureValue<int>();

            // Post to show
            Post postEntity = null!;
            User postAuthor = null!;
            var postQuery = _session.QueryOver(() => postEntity)
                .JoinEntityAlias(() => postAuthor, () => postAuthor.ID == postEntity.AuthorID)
                .Where(() => postEntity.ID == request.PostID)
                .SelectList(list => list
                    .Select(() => postEntity.ID).WithAlias(() => post.PostID)
                    .Select(() => postEntity.Text).WithAlias(() => post.Text)
                    .Select(() => postEntity.Picture).WithAlias(() => post.Picture)
                    .Select(() => postEntity.PostDate).WithAlias(() => post.PostDate)
                    .Select(() => postAuthor.Nickname).WithAlias(() => post.AuthorNickName)
                    .Select(() => postAuthor.ProfilePicture).WithAlias(() => post.AuthorProfilePicture)
                )
                .TransformUsing(new EntityToModelResultTransformer<PostModel>())
                .FutureValue<PostModel>();

            // Add related data
            post = await postQuery.GetValueAsync(cancellationToken);
            post.LikesCount = await postLikesCountQuery.GetValueAsync(cancellationToken);
            post.Comments = postCommentsQuery.ToArray();
            post.IsLikedByCurrentUser = await isLikedByCurrentUserQuery.GetValueAsync(cancellationToken) > 0;
            post.CanBeEditedByCurrentUser =
                (await _session.LoadAsync<Post>(request.PostID, cancellationToken)).CanBeEditedBy(request.CurrentUserID);
        }

        return post;
    }
}
