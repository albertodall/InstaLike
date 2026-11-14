using System;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using InstaLike.Core.Posts;
using InstaLike.Web.Data;
using InstaLike.Web.Media;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using NHibernate;
using Serilog;

namespace InstaLike.Web.Posts
{
    [Bind("AuthorID, Text")]
    public sealed class PublishPostModel
    {
        public Guid AuthorID { get; set; }

        [Required(ErrorMessage = "Please, write a text for the picture you're publishing.")]
        public string Text { get; set; } = string.Empty;
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
                return Result.Failure<PostId>("Please select a picture to share together with your post.");
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

                return Result.Failure<PostId>(ex.Message);
            }

            _logger.Information("User {UserID} has just shared post {PostID}.", request.UserID, post.ID);

            return Result.Success(post.ID);
        }
    }
}
