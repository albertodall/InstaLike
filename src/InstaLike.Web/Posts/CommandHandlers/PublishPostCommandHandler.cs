using System;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using InstaLike.Core.Accounts;
using InstaLike.Core.Posts;
using InstaLike.Core.SharedKernel;
using InstaLike.Web.Media;
using MediatR;
using NHibernate;
using Serilog;

namespace InstaLike.Web.Posts.CommandHandlers;

internal sealed class PublishPostCommandHandler : IRequestHandler<PublishPostCommand, Result<int>>
{
    private readonly ISession _session;
    private readonly ILogger _logger;
    private readonly IPictureStore _pictureStore;

    public PublishPostCommandHandler(ISession session, ILogger logger, IPictureStore pictureStore)
    {
        _session = session ?? throw new ArgumentNullException(nameof(session));
        _logger = logger?.ForContext<PublishPostCommand>() ?? throw new ArgumentNullException(nameof(logger));
        _pictureStore = pictureStore ?? throw new ArgumentNullException(nameof(pictureStore));
    }

    public async Task<Result<int>> Handle(PublishPostCommand request, CancellationToken cancellationToken)
    {
        if (request.PictureRawBytes is not { Length: > 0 })
        {
            return Result.Failure<int>("Please select a picture to share together with your post.");
        }

        using (var tx = _session.BeginTransaction())
        {
            User? author = null;
            PictureId? postPicture = null;
            try
            {
                author = await _session.LoadAsync<User>(request.UserID, cancellationToken);
                postPicture = await _pictureStore.SaveAsync(PictureContainers.Posts, request.PictureRawBytes, cancellationToken);

                var post = new Post(author, postPicture, (PostText)request.Text);

                await _session.SaveAsync(post, cancellationToken);
                await tx.CommitAsync(cancellationToken);

                _logger.Information("User [{Nickname}({UserID})] has just shared a new post.",
                    author.Nickname,
                    request.UserID);

                return Result.Success(post.ID);
            }
            catch (ADOException ex)
            {
                await tx.RollbackAsync(cancellationToken);
                await _pictureStore.TryDeleteAsync(PictureContainers.Posts, postPicture);

                _logger.Error("Failed to publish a post for user [{Nickname}({UserID})]. Error message: {ErrorMessage}",
                    author?.Nickname,
                    request.UserID,
                    ex.Message);

                return Result.Failure<int>(ex.Message);
            }
        }
    }
}
