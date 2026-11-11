using System;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using InstaLike.Core.Posts;
using InstaLike.Web.Posts.Commands;
using MediatR;
using NHibernate;
using Serilog;

namespace InstaLike.Web.Posts.CommandHandlers;

internal sealed class LikeOrDislikePostCommandHandler : IRequestHandler<LikePostCommand, Result>, IRequestHandler<DislikePostCommand, Result>
{
    private readonly ISession _session;
    private readonly ILogger _logger;

    public LikeOrDislikePostCommandHandler(ISession session, ILogger logger)
    {
        _session = session ?? throw new ArgumentNullException(nameof(session));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result> Handle(LikePostCommand request, CancellationToken cancellationToken)
    {
        var logger = _logger.ForContext<LikePostCommand>();

        var post = await _session.GetAsync<Post>(request.PostID, cancellationToken);
        if (post is null)
        {
            return Result.Failure($"Post {request.PostID} does not exist.");
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

            return Result.Failure(ex.Message);
        }

        logger.Information("User {UserID} put a 'Like' on post {PostID}", request.UserID, request.PostID);

        return Result.Success();
    }

    public async Task<Result> Handle(DislikePostCommand request, CancellationToken cancellationToken)
    {
        var logger = _logger.ForContext<DislikePostCommand>();

        var post = await _session.GetAsync<Post>(request.PostID, cancellationToken);
        if (post is null)
        {
            return Result.Failure($"Post {request.PostID} does not exist.");
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

            return Result.Failure(ex.Message);
        }

        logger.Information("User {UserID} removed a 'Like' on post {PostID}", request.UserID, request.PostID);

        return Result.Success();
    }
}
