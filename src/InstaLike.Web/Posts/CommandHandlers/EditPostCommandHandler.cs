using System;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using InstaLike.Core.Accounts;
using InstaLike.Core.Posts;
using InstaLike.Web.Media;
using MediatR;
using NHibernate;
using Serilog;

namespace InstaLike.Web.Posts.CommandHandlers;

internal sealed class EditPostCommandHandler : IRequestHandler<EditPostCommand, Result<int>>
{
    private readonly ISession _session;
    private readonly ILogger _logger;
    private readonly IPictureStore _pictureStore;

    public EditPostCommandHandler(ISession session, ILogger logger, IPictureStore pictureStore)
    {
        _session = session ?? throw new ArgumentNullException(nameof(session));
        _logger = logger?.ForContext<PublishPostCommand>() ?? throw new ArgumentNullException(nameof(logger));
        _pictureStore = pictureStore ?? throw new ArgumentNullException(nameof(pictureStore));
    }

    public async Task<Result<int>> Handle(EditPostCommand request, CancellationToken cancellationToken)
    {
        if (request.PictureRawBytes is not { Length: > 0 })
        {
            return Result.Failure<int>("Please select a picture to share together with your post.");
        }

        using (var tx = _session.BeginTransaction())
        {
            var editor = await _session.LoadAsync<User>(request.UserID, cancellationToken);
            Post? postToEdit = await _session.QueryOver<Post>()
                .Fetch(SelectMode.ChildFetch, p => p.Author)
                .Where(p => p.ID == request.PostID)
                .SingleOrDefaultAsync(cancellationToken);

            if (postToEdit is null)
            {
                return Result.Failure<int>($"Post {request.PostID} does not exist.");
            }

            if (!postToEdit.CanBeEditedBy(editor))
            {
                _logger.Warning("User [{NickName}({UserID})] tried to edit post {PostID} but he wasn't allowed to.",
                    editor.Nickname,
                    request.UserID,
                    request.PostID);
                return Result.Failure<int>($"You're not allowed to edit post {postToEdit.ID}.");
            }

            postToEdit.UpdateText(PostText.Create(request.Text).Value);
            var replacedPicture = postToEdit.Picture;
            var uploadedPicture = await _pictureStore.SaveAsync(PictureContainers.Posts, request.PictureRawBytes, cancellationToken);
            postToEdit.UpdatePicture(uploadedPicture);
            try
            {
                await _session.SaveAsync(postToEdit, cancellationToken);
                await tx.CommitAsync(cancellationToken);
                await _pictureStore.TryDeleteAsync(PictureContainers.Posts, replacedPicture);

                _logger.Information("User [{Nickname}({UserID})] edited post {PostID}.",
                    editor.Nickname,
                    request.UserID,
                    request.PostID);

                return Result.Success(postToEdit.ID);
            }
            catch (ADOException ex)
            {
                await tx.RollbackAsync(cancellationToken);
                await _pictureStore.TryDeleteAsync(PictureContainers.Posts, uploadedPicture);

                _logger.Error("Failed to edit post {PostID} for user [{Nickname}({UserID})]. Error message: {ErrorMessage}",
                    request.PostID,
                    editor.Nickname,
                    request.UserID,
                    ex.Message);

                return Result.Failure<int>(ex.Message);
            }
        }
    }
}
