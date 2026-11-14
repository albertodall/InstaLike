using System;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
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
    [Bind("PostID, AuthorID, Text")]
    public sealed class EditPostModel
    {
        public required PostId PostID { get; set; }

        public Guid AuthorID { get; set; }

        [Required(ErrorMessage = "Please, write a text for the picture you're publishing.")]
        public string Text { get; set; } = string.Empty;

        public PictureId? Picture { get; set; }
    }

    public sealed class EditPostQuery : IRequest<EditPostModel>
    {
        public PostId PostID { get; }

        public Guid CurrentUserID { get; }

        public EditPostQuery(PostId postID, Guid currentUserID)
        {
            PostID = postID;
            CurrentUserID = currentUserID;
        }
    }

    internal sealed class EditPostQueryHandler : IRequestHandler<EditPostQuery, EditPostModel>
    {
        private readonly IStatelessSession _session;
        private readonly ILogger _logger;

        public EditPostQueryHandler(IStatelessSession session, ILogger logger)
        {
            _session = session;
            _logger = logger.ForContext<EditPostQuery>();
        }

        public async Task<EditPostModel> Handle(EditPostQuery request, CancellationToken cancellationToken)
        {
            _logger.Debug("User {UserID} requested to edit post {PostID} with parameters {@Request}", request.CurrentUserID, request.PostID, request);

            using (_session.BeginTransaction())
            {
                var post = await _session.GetAsync<Post>(request.PostID, cancellationToken);

                return new EditPostModel()
                {
                    PostID = post.ID,
                    AuthorID = post.AuthorID,
                    Picture = post.Picture,
                    Text = post.Text
                };
            }
        }
    }

    public sealed class EditPostCommand : ICommand<Result<PostId>>
    {
        public Guid UserID { get; }
        public PostId PostID { get; }
        public string Text { get; }
        public byte[] PictureRawBytes { get; }

        public EditPostCommand(Guid userID, PostId postID, string text, byte[] pictureRawBytes)
        {
            UserID = userID;
            PostID = postID;
            Text = text;
            PictureRawBytes = pictureRawBytes;
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
            if (request.PictureRawBytes is not { Length: > 0 })
            {
                return Result.Failure<PostId>("Please select a picture to share together with your post.");
            }

            var text = PostText.Create(request.Text);
            if (text.IsFailure)
            {
                return Result.Failure<PostId>(text.Error);
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

            postToEdit.UpdateText(text.Value);
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
}
