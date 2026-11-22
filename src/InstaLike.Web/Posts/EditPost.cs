using System;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using InstaLike.Core.Posts;
using InstaLike.Core.SharedKernel;
using InstaLike.Web.Data;
using InstaLike.Web.Media;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using NHibernate;
using Serilog;

namespace InstaLike.Web.Posts
{
    [Bind("PostID, Text, Picture, PictureFile")]
    public sealed class EditPostModel
    {
        public required PostId PostID { get; set; }

        [Required(ErrorMessage = "Please, write a text for the picture you're publishing.")]
        public string Text { get; set; } = string.Empty;

        public PictureId? Picture { get; set; }

        public Microsoft.AspNetCore.Http.IFormFile? PictureFile { get; set; }
    }

    public sealed class EditPostQuery : IRequest<Result<EditPostModel>>
    {
        public PostId PostID { get; }

        public Guid CurrentUserID { get; }

        public EditPostQuery(PostId postID, Guid currentUserID)
        {
            PostID = postID;
            CurrentUserID = currentUserID;
        }
    }

    internal sealed class EditPostQueryHandler : IRequestHandler<EditPostQuery, Result<EditPostModel>>
    {
        private readonly IStatelessSession _session;
        private readonly ILogger _logger;

        public EditPostQueryHandler(IStatelessSession session, ILogger logger)
        {
            _session = session;
            _logger = logger.ForContext<EditPostQuery>();
        }

        public async Task<Result<EditPostModel>> Handle(EditPostQuery request, CancellationToken cancellationToken)
        {
            _logger.Debug("User {UserID} requested to edit post {PostID} with parameters {@Request}", request.CurrentUserID, request.PostID, request);

            using (_session.BeginTransaction())
            {
                var post = await _session.GetAsync<Post>(request.PostID, cancellationToken);
                if (post is null)
                {
                    return Result.Failure<EditPostModel>(PostErrors.NotFound(request.PostID));
                }

                if (!post.CanBeEditedBy(request.CurrentUserID))
                {
                    return Result.Failure<EditPostModel>(PostErrors.EditNotAllowed);
                }

                return Result.Ok(new EditPostModel()
                {
                    PostID = post.ID,
                    Picture = post.Picture,
                    Text = post.Text
                });
            }
        }
    }

    public sealed class EditPostCommand : ICommand<Result<PostId>>
    {
        public Guid UserID { get; }
        public PostId PostID { get; }
        public string Text { get; }
        public byte[]? NewPictureRawBytes { get; }

        public EditPostCommand(Guid userID, PostId postID, string text, byte[]? newPictureRawBytes)
        {
            UserID = userID;
            PostID = postID;
            Text = text;
            NewPictureRawBytes = newPictureRawBytes;
        }
    }

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
            if (request.NewPictureRawBytes is { Length: 0 })
            {
                return Result.Failure<PostId>(PostErrors.PictureMissing);
            }

            var text = PostText.Create(request.Text);
            if (text.IsFailure)
            {
                return Result.Failure<PostId>(text.Error);
            }

            var postToEdit = await _session.GetAsync<Post>(request.PostID, cancellationToken);

            if (postToEdit is null)
            {
                return Result.Failure<PostId>(PostErrors.NotFound(request.PostID));
            }

            if (!postToEdit.CanBeEditedBy(request.UserID))
            {
                _logger.Warning("User {UserID} tried to edit post {PostID} but was not allowed to.",
                    request.UserID,
                    request.PostID);
                return Result.Failure<PostId>(PostErrors.EditNotAllowed);
            }

            postToEdit.UpdateText(text.Value);
            if (request.NewPictureRawBytes is not null)
            {
                var uploadedPicture = await _pictureStore.SaveAsync(PictureContainers.Posts, request.NewPictureRawBytes, cancellationToken);
                postToEdit.UpdatePicture(uploadedPicture);
            }

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

                return Result.Failure<PostId>(DataErrors.SaveFailed(ex.Message));
            }

            _logger.Information("User {UserID} edited post {PostID}.", request.UserID, request.PostID);

            return Result.Ok(postToEdit.ID);
        }
    }
}
