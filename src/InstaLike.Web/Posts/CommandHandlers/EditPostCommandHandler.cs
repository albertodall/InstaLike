using System;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using InstaLike.Core.Posts;
using InstaLike.Web.Data;
using InstaLike.Web.Media;
using InstaLike.Web.Posts.Commands;
using MediatR;
using NHibernate;
using Serilog;

namespace InstaLike.Web.Posts.CommandHandlers;

internal sealed class EditPostCommandHandler : IRequestHandler<EditPostCommand, Result<int>>
{
    private readonly ISession _session;
    private readonly ILogger _logger;
    private readonly IPictureStore _pictureStore;
    private readonly IUnitOfWork _unitOfWork;

    public EditPostCommandHandler(ISession session, ILogger logger, IPictureStore pictureStore, IUnitOfWork unitOfWork)
    {
        _session = session ?? throw new ArgumentNullException(nameof(session));
        _logger = logger?.ForContext<EditPostCommand>() ?? throw new ArgumentNullException(nameof(logger));
        _pictureStore = pictureStore ?? throw new ArgumentNullException(nameof(pictureStore));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<Result<int>> Handle(EditPostCommand request, CancellationToken cancellationToken)
    {
        if (request.PictureRawBytes is not { Length: > 0 })
        {
            return Result.Failure<int>("Please select a picture to share together with your post.");
        }

        var postToEdit = await _session.GetAsync<Post>(request.PostID, cancellationToken);

        if (postToEdit is null)
        {
            return Result.Failure<int>($"Post {request.PostID} does not exist.");
        }

        if (!postToEdit.CanBeEditedBy(request.UserID))
        {
            _logger.Warning("User {UserID} tried to edit post {PostID} but was not allowed to.",
                request.UserID,
                request.PostID);
            return Result.Failure<int>($"You're not allowed to edit post {postToEdit.ID}.");
        }

        postToEdit.UpdateText(PostText.Create(request.Text).Value);
        var replacedPicture = postToEdit.Picture;
        var uploadedPicture = await _pictureStore.SaveAsync(PictureContainers.Posts, request.PictureRawBytes, cancellationToken);
        _unitOfWork.OnCommitted(() => _pictureStore.TryDeleteAsync(PictureContainers.Posts, replacedPicture));
        _unitOfWork.OnRolledBack(() => _pictureStore.TryDeleteAsync(PictureContainers.Posts, uploadedPicture));
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

            return Result.Failure<int>(ex.Message);
        }

        _logger.Information("User {UserID} edited post {PostID}.", request.UserID, request.PostID);

        return Result.Success(postToEdit.ID);
    }
}
