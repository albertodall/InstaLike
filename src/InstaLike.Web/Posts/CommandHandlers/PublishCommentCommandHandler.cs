using System;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using InstaLike.Core.Posts;
using MediatR;
using NHibernate;
using Serilog;

namespace InstaLike.Web.Posts.CommandHandlers;

internal sealed class PublishCommentCommandHandler : IRequestHandler<PublishCommentCommand, Result<int>>
{
    private readonly ISession _session;
    private readonly ILogger _logger;

    public PublishCommentCommandHandler(ISession session, ILogger logger)
    {
        _session = session ?? throw new ArgumentNullException(nameof(session));
        _logger = logger?.ForContext<PublishCommentCommand>() ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<int>> Handle(PublishCommentCommand request, CancellationToken cancellationToken)
    {
        var text = CommentText.Create(request.Text);
        if (text.IsFailure)
        {
            return Result.Failure<int>(text.Error);
        }

        var post = await _session.GetAsync<Post>(request.PostID, cancellationToken);
        if (post is null)
        {
            return Result.Failure<int>($"Post {request.PostID} does not exist.");
        }

        var comment = post.CommentBy(request.UserID, text.Value);
        try
        {
            await _session.SaveAsync(comment, cancellationToken);
            await _session.FlushAsync(cancellationToken);
        }
        catch (ADOException ex)
        {
            _logger.Error("Failed to save comment to post {PostID} written by user {UserID}. Error message: {ErrorMessage}",
                request.PostID,
                request.UserID,
                ex.Message);

            return Result.Failure<int>(ex.Message);
        }

        _logger.Information("User {UserID} wrote a new comment to post {PostID}", request.UserID, request.PostID);

        return Result.Success(comment.ID);
    }
}
