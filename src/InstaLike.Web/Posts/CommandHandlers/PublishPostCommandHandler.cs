using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using InstaLike.Core.Posts;
using InstaLike.Web.Media;
using InstaLike.Web.Posts.Commands;
using MediatR;
using NHibernate;
using Serilog;

namespace InstaLike.Web.Posts.CommandHandlers;

internal sealed class PublishPostCommandHandler : IRequestHandler<PublishPostCommand, Result<PostId>>
{
    private readonly ISession _session;
    private readonly ILogger _logger;
    private readonly IPictureStore _pictureStore;

    public PublishPostCommandHandler(ISession session, ILogger logger, IPictureStore pictureStore)
    {
        _session = session;
        _logger = logger.ForContext<PublishPostCommand>();
        _pictureStore = pictureStore;
    }

    public async Task<Result<PostId>> Handle(PublishPostCommand request, CancellationToken cancellationToken)
    {
        if (request.PictureRawBytes is not { Length: > 0 })
        {
            return Result.Failure<PostId>("Please select a picture to share together with your post.");
        }

        var postPicture = await _pictureStore.SaveAsync(PictureContainers.Posts, request.PictureRawBytes, cancellationToken);

        var post = new Post(request.UserID, postPicture, (PostText)request.Text);
        try
        {
            await _session.SaveAsync(post, cancellationToken);
            await _session.FlushAsync(cancellationToken);
        }
        catch (ADOException ex)
        {
            _logger.Error("Failed to publish a post for user {UserID}. Error message: {ErrorMessage}",
                request.UserID,
                ex.Message);

            return Result.Failure<PostId>(ex.Message);
        }

        _logger.Information("User {UserID} has just shared post {PostID}.", request.UserID, post.ID);

        return Result.Success(post.ID);
    }
}
