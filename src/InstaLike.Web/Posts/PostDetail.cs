using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using InstaLike.Core.Membership;
using InstaLike.Core.Posts;
using InstaLike.Core.SharedKernel;
using InstaLike.Web.Data;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.SqlCommand;
using Serilog;

namespace InstaLike.Web.Posts
{
    public sealed class PostDetailModel
    {
        [HiddenInput]
        public required PostId PostID { get; set; }
        public string AuthorNickName { get; set; } = string.Empty;
        public PictureId? AuthorProfilePicture { get; set; }
        public DateTimeOffset PostDate { get; set; }
        public PictureId? Picture { get; set; }
        public string Text { get; set; } = string.Empty;
        public CommentModel[] Comments { get; set; } = [];
        public int LikesCount { get; set; }
        public bool IsLikedByCurrentUser { get; set; }
        public bool CanBeLikedByCurrentUser { get; set; }
        public bool CanBeEditedByCurrentUser { get; set; }
    }

    public sealed class PostDetailQuery : IRequest<Result<PostDetailModel>>
    {
        public PostId PostID { get; }
        public MemberId CurrentUserID { get; }

        public PostDetailQuery(PostId postID, MemberId currentUserID)
        {
            PostID = postID;
            CurrentUserID = currentUserID;
        }
    }

    internal sealed class PostDetailQueryHandler : IRequestHandler<PostDetailQuery, Result<PostDetailModel>>
    {
        private readonly IStatelessSession _session;
        private readonly ILogger _logger;

        public PostDetailQueryHandler(IStatelessSession session, ILogger logger)
        {
            _session = session;
            _logger = logger.ForContext<PostDetailQuery>();
        }

        public async Task<Result<PostDetailModel>> Handle(PostDetailQuery request, CancellationToken cancellationToken)
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
                var postCommentsQuery = _session.QueryOver(() => commentEntity)
                    .JoinEntityAlias(() => commentAuthor, Restrictions.EqProperty(Projections.Property(() => commentAuthor.ID), Projections.Property(() => commentEntity.AuthorID)), JoinType.InnerJoin)
                    .Where(() => commentEntity.Post.ID == request.PostID)
                    .OrderBy(() => commentEntity.CommentDate).Asc
                    .SelectInto(() => new CommentModel
                    {
                        Text = commentEntity.Text,
                        CommentDate = commentEntity.CommentDate,
                        AuthorNickName = commentAuthor.Nickname
                    })
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
                var postModelQuery = _session.QueryOver(() => postEntity)
                    .JoinEntityAlias(() => postAuthor,
                        Restrictions.EqProperty(
                            Projections.Property(() => postAuthor.ID),
                            Projections.Property(() => postEntity.AuthorID)),
                        JoinType.InnerJoin)
                    .Where(() => postEntity.ID == request.PostID)
                    .SelectInto(() => new PostDetailModel
                    {
                        PostID = postEntity.ID,
                        AuthorNickName = postAuthor.Nickname,
                        AuthorProfilePicture = postAuthor.ProfilePicture
                    })
                    .FutureValue<PostDetailModel>();

                var post = await postQuery.GetValueAsync(cancellationToken);
                if (post is null)
                {
                    return Result.Failure<PostDetailModel>(PostErrors.NotFound(request.PostID));
                }

                var model = await postModelQuery.GetValueAsync(cancellationToken);
                model.Picture = post.Picture;
                model.PostDate = post.PostDate;
                model.Text = post.Text;
                model.Comments = postCommentsQuery.ToArray();
                model.LikesCount = await postLikesCountQuery.GetValueAsync(cancellationToken);
                model.IsLikedByCurrentUser = await isLikedByCurrentUserQuery.GetValueAsync(cancellationToken) > 0;
                model.CanBeLikedByCurrentUser = post.CanBeLikedBy(request.CurrentUserID);
                model.CanBeEditedByCurrentUser = post.CanBeEditedBy(request.CurrentUserID);

                return Result.Ok(model);
            }
        }
    }
}
