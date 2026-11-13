using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using InstaLike.Core.Accounts;
using InstaLike.Core.Membership;
using InstaLike.Core.Posts;
using InstaLike.Core.SharedKernel;
using InstaLike.Web.Models;
using MediatR;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.SqlCommand;
using NHibernate.Transform;
using Serilog;

namespace InstaLike.Web.Posts.Queries;

public sealed class PostDetailQuery : IRequest<PostModel>
{
    public PostId PostID { get; }
    public Guid CurrentUserID { get; }

    public PostDetailQuery(PostId postID, Guid currentUserID)
    {
        PostID = postID;
        CurrentUserID = currentUserID;
    }
}

internal sealed class PostDetailQueryHandler : IRequestHandler<PostDetailQuery, PostModel>
{
    private readonly IStatelessSession _session;
    private readonly ILogger _logger;

    public PostDetailQueryHandler(IStatelessSession session, ILogger logger)
    {
        _session = session;
        _logger = logger.ForContext<PostDetailQuery>();
    }

    public async Task<PostModel> Handle(PostDetailQuery request, CancellationToken cancellationToken)
    {
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
            Member commentAuthor = null!;
            CommentModel comment = null!;
            var postCommentsQuery = _session.QueryOver(() => commentEntity)
                .JoinEntityAlias(() => commentAuthor, Restrictions.EqProperty(Projections.Property(() => commentAuthor.ID), Projections.Property(() => commentEntity.AuthorID)), JoinType.InnerJoin)
                .Where(() => commentEntity.Post.ID == request.PostID)
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
                    .And(l => l.MemberID == request.CurrentUserID)
                .ToRowCountQuery()
                .FutureValue<int>();

            // Post to show
            var postQuery = _session.QueryOver<Post>()
                .Where(p => p.ID == request.PostID)
                .FutureValue();

            Post postEntity = null!;
            Member postAuthor = null!;
            var authorQuery = _session.QueryOver(() => postEntity)
                .JoinEntityAlias(() => postAuthor, Restrictions.EqProperty(Projections.Property(() => postAuthor.ID), Projections.Property(() => postEntity.AuthorID)), JoinType.InnerJoin)
                .Where(() => postEntity.ID == request.PostID)
                .SelectList(list => list
                    .Select(() => postAuthor.Nickname)
                    .Select(() => postAuthor.ProfilePicture)
                )
                .FutureValue<object[]>();

            var post = await postQuery.GetValueAsync(cancellationToken);
            var author = await authorQuery.GetValueAsync(cancellationToken);

            return new PostModel
            {
                PostID = post.ID,
                AuthorNickName = (string)author[0],
                AuthorProfilePicture = (PictureId?)author[1],
                Picture = post.Picture,
                PostDate = post.PostDate,
                Text = post.Text,
                Comments = postCommentsQuery.ToArray(),
                LikesCount = await postLikesCountQuery.GetValueAsync(cancellationToken),
                IsLikedByCurrentUser = await isLikedByCurrentUserQuery.GetValueAsync(cancellationToken) > 0,
                CanBeLikedByCurrentUser = post.CanBeLikedBy(request.CurrentUserID),
                CanBeEditedByCurrentUser = post.CanBeEditedBy(request.CurrentUserID)
            };
        }
    }
}
