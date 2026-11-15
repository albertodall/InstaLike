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
    [Bind("AuthorID, Text, PictureFile")]
    public sealed class PublishPostModel
    {
        public Guid AuthorID { get; set; }

        [Required(ErrorMessage = "Please, write a text for the picture you're publishing.")]
        public string Text { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please select a picture to share together with your post.")]
        public Microsoft.AspNetCore.Http.IFormFile? PictureFile { get; set; }
    }

    public sealed class PublishPostCommand : ICommand<Result<PostId>>
    {
        public Guid UserID { get; }
        public string Text { get; }
        public byte[] PictureRawBytes { get; }

        public PublishPostCommand(Guid userID, string text, byte[] pictureRawBytes)
        {
            UserID = userID;
            Text = text;
            PictureRawBytes = pictureRawBytes;
        }
    }

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
                return Result.Failure<PostId>(PostErrors.PictureMissing);
            }

            var text = PostText.Create(request.Text);
            if (text.IsFailure)
            {
                return Result.Failure<PostId>(text.Error);
            }

            var postPicture = await _pictureStore.SaveAsync(PictureContainers.Posts, request.PictureRawBytes, cancellationToken);

            var post = new Post(request.UserID, postPicture, text.Value);
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

                return Result.Failure<PostId>(DataErrors.SaveFailed(ex.Message));
            }

            _logger.Information("User {UserID} has just shared post {PostID}.", request.UserID, post.ID);

            return Result.Ok(post.ID);
        }
    }
}
