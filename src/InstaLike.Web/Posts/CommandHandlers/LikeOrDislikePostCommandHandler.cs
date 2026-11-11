using System;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using InstaLike.Core.Posts;
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

        using (var tx = _session.BeginTransaction())
        {
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

            if (await FindLikeAsync(request.PostID, request.UserID, cancellationToken) is not null)
            {
                return Result.Failure($"User {request.UserID} already 'Liked' post {request.PostID}.");
            }

            try
            {
                await _session.SaveAsync(like.Value, cancellationToken);
                await tx.CommitAsync(cancellationToken);

                logger.Information("User {UserID} put a 'Like' on post {PostID}", request.UserID, request.PostID);

                return Result.Success();
            }
            catch (ADOException ex)
            {
                await tx.RollbackAsync(cancellationToken);

                logger.Error("Failed to put a 'Like' on post {PostID} by user {UserID}. Error message: {ErrorMessage}",
                    request.PostID,
                    request.UserID,
                    ex.Message);

                return Result.Failure(ex.Message);
            }
        }
    }

    public async Task<Result> Handle(DislikePostCommand request, CancellationToken cancellationToken)
    {
        var logger = _logger.ForContext<DislikePostCommand>();

        using (var tx = _session.BeginTransaction())
        {
            var like = await FindLikeAsync(request.PostID, request.UserID, cancellationToken);
            if (like is null)
            {
                return Result.Failure($"User {request.UserID} did not put any 'Like' on post {request.PostID}.");
            }

            try
            {
                await _session.DeleteAsync(like, cancellationToken);
                await tx.CommitAsync(cancellationToken);

                logger.Information("User {UserID} removed a 'Like' on post {PostID}", request.UserID, request.PostID);

                return Result.Success();
            }
            catch (ADOException ex)
            {
                await tx.RollbackAsync(cancellationToken);

                logger.Error("Failed to remove a 'Like' on post {PostID} by user {UserID}. Error message: {ErrorMessage}",
                    request.PostID,
                    request.UserID,
                    ex.Message);

                return Result.Failure(ex.Message);
            }
        }
    }

    private Task<Like?> FindLikeAsync(int postID, int userID, CancellationToken cancellationToken)
    {
        return _session.QueryOver<Like>()
            .Where(l => l.PostID == postID && l.UserID == userID)
            .SingleOrDefaultAsync<Like?>(cancellationToken);
    }
}
