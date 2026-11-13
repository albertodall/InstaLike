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

internal sealed class EditPostCommandHandler : IRequestHandler<EditPostCommand, Result<PostId>>
{
    private readonly ISession _session;
    private readonly ILogger _logger;
    private readonly IPictureStore _pictureStore;

    public EditPostCommandHandler(ISession session, ILogger logger, IPictureStore pictureStore)
    {
        _session = session;
        _logger = logger.ForContext<EditPostCommand>();
        _pictureStore = pictureStore;
    }

    public async Task<Result<PostId>> Handle(EditPostCommand request, CancellationToken cancellationToken)
    {
        if (request.PictureRawBytes is not { Length: > 0 })
        {
            return Result.Failure<PostId>("Please select a picture to share together with your post.");
        }

        var postToEdit = await _session.GetAsync<Post>(request.PostID, cancellationToken);

        if (postToEdit is null)
        {
            return Result.Failure<PostId>($"Post {request.PostID} does not exist.");
        }

        if (!postToEdit.CanBeEditedBy(request.UserID))
        {
            _logger.Warning("User {UserID} tried to edit post {PostID} but was not allowed to.",
                request.UserID,
                request.PostID);
            return Result.Failure<PostId>($"You're not allowed to edit post {postToEdit.ID}.");
        }

        postToEdit.UpdateText(PostText.Create(request.Text).Value);
        var uploadedPicture = await _pictureStore.SaveAsync(PictureContainers.Posts, request.PictureRawBytes, cancellationToken);
        postToEdit.UpdatePicture(uploadedPicture);

        try
        {
            await _session.FlushAsync(cancellationToken);
        }
        catch (ADOException ex)
        {
            _logger.Error("Failed to edit post {PostID} for user {UserID}. Error message: {ErrorMessage}",
                request.PostID,
                request.UserID,
                ex.Message);

            return Result.Failure<PostId>(ex.Message);
        }

        _logger.Information("User {UserID} edited post {PostID}.", request.UserID, request.PostID);

        return Result.Success(postToEdit.ID);
    }
}
