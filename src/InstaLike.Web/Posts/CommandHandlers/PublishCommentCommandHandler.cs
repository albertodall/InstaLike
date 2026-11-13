using System;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using InstaLike.Core.Posts;
using InstaLike.Web.Posts.Commands;
using MediatR;
using NHibernate;
using Serilog;

namespace InstaLike.Web.Posts.CommandHandlers
{
    internal sealed class PublishCommentCommandHandler : IRequestHandler<PublishCommentCommand, Result<Guid>>
    {
        private readonly ISession _session;
        private readonly ILogger _logger;

        public PublishCommentCommandHandler(ISession session, ILogger logger)
        {
            _session = session;
            _logger = logger.ForContext<PublishCommentCommand>();
        }

        public async Task<Result<Guid>> Handle(PublishCommentCommand request, CancellationToken cancellationToken)
        {
            var text = CommentText.Create(request.Text);
            if (text.IsFailure)
            {
                return Result.Failure<Guid>(text.Error);
            }

            var post = await _session.GetAsync<Post>(request.PostID, cancellationToken);
            if (post is null)
            {
                return Result.Failure<Guid>($"Post {request.PostID} does not exist.");
            }

            var comment = post.CommentBy(request.UserID, text.Value);
            try
            {
                await _session.FlushAsync(cancellationToken);
            }
            catch (ADOException ex)
            {
                _logger.Error("Failed to save comment to post {PostID} written by user {UserID}. Error message: {ErrorMessage}",
                    request.PostID,
                    request.UserID,
                    ex.Message);

                return Result.Failure<Guid>(ex.Message);
            }

            _logger.Information("User {UserID} wrote a new comment to post {PostID}", request.UserID, request.PostID);

            return Result.Success(comment.ID);
        }
    }
}
