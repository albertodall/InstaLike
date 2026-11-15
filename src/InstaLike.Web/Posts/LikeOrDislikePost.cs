using System;
using System.Threading;
using System.Threading.Tasks;
using InstaLike.Core.Posts;
using InstaLike.Core.SharedKernel;
using InstaLike.Web.Data;
using MediatR;
using NHibernate;
using Serilog;

namespace InstaLike.Web.Posts
{
    public sealed class LikePostModel
    {
        public required PostId PostID { get; set; }
    }

    public sealed class LikePostCommand : ICommand<Result>
    {
        public PostId PostID { get; }
        public Guid UserID { get; }

        public LikePostCommand(PostId postID, Guid userID)
        {
            PostID = postID;
            UserID = userID;
        }
    }

    public sealed class DislikePostCommand : ICommand<Result>
    {
        public PostId PostID { get; }
        public Guid UserID { get; }

        public DislikePostCommand(PostId postID, Guid userID)
        {
            PostID = postID;
            UserID = userID;
        }
    }

    internal sealed class LikeOrDislikePostCommandHandler : IRequestHandler<LikePostCommand, Result>, IRequestHandler<DislikePostCommand, Result>
    {
        private static readonly Error PostNotFound = Error.NotFound("Post.NotFound", "This post does not exist.");
        private static readonly Error LikeNotSaved = Error.Failure("Like.NotSaved", "Your like could not be saved. Please try again.");
        private static readonly Error LikeNotRemoved = Error.Failure("Like.NotRemoved", "Your like could not be removed. Please try again.");

        private readonly ISession _session;
        private readonly ILogger _logger;

        public LikeOrDislikePostCommandHandler(ISession session, ILogger logger)
        {
            _session = session;
            _logger = logger;
        }

        public async Task<Result> Handle(LikePostCommand request, CancellationToken cancellationToken)
        {
            var logger = _logger.ForContext<LikePostCommand>();

            var post = await _session.GetAsync<Post>(request.PostID, cancellationToken);
            if (post is null)
            {
                return Result.Failure(PostNotFound);
            }

            var like = post.LikeBy(request.UserID);
            if (like.IsFailure)
            {
                return like;
            }

            try
            {
                await _session.FlushAsync(cancellationToken);
            }
            catch (ADOException ex)
            {
                logger.Error("Failed to put a 'Like' on post {PostID} by user {UserID}. Error message: {ErrorMessage}",
                    request.PostID,
                    request.UserID,
                    ex.Message);

                return Result.Failure(LikeNotSaved);
            }

            logger.Information("User {UserID} put a 'Like' on post {PostID}", request.UserID, request.PostID);

            return Result.Ok();
        }

        public async Task<Result> Handle(DislikePostCommand request, CancellationToken cancellationToken)
        {
            var logger = _logger.ForContext<DislikePostCommand>();

            var post = await _session.GetAsync<Post>(request.PostID, cancellationToken);
            if (post is null)
            {
                return Result.Failure(PostNotFound);
            }

            var dislike = post.DislikeBy(request.UserID);
            if (dislike.IsFailure)
            {
                return dislike;
            }

            try
            {
                await _session.FlushAsync(cancellationToken);
            }
            catch (ADOException ex)
            {
                logger.Error("Failed to remove a 'Like' on post {PostID} by user {UserID}. Error message: {ErrorMessage}",
                    request.PostID,
                    request.UserID,
                    ex.Message);

                return Result.Failure(LikeNotRemoved);
            }

            logger.Information("User {UserID} removed a 'Like' on post {PostID}", request.UserID, request.PostID);

            return Result.Ok();
        }
    }
}
