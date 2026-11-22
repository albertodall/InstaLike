using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using InstaLike.Core.Posts;
using InstaLike.Core.SharedKernel;
using InstaLike.Web.Media;
using MediatR;
using NHibernate;
using Serilog;

namespace InstaLike.Web.Posts
{
    public sealed class SuggestPictureTagsQuery : IRequest<Result<string[]>>
    {
        public Guid CurrentUserID { get; }
        public PostId? PostID { get; }
        public byte[]? NewPictureRawBytes { get; }

        public SuggestPictureTagsQuery(Guid currentUserID, PostId? postID, byte[]? newPictureRawBytes)
        {
            CurrentUserID = currentUserID;
            PostID = postID;
            NewPictureRawBytes = newPictureRawBytes;
        }
    }

    internal sealed class SuggestPictureTagsQueryHandler : IRequestHandler<SuggestPictureTagsQuery, Result<string[]>>
    {
        private readonly IStatelessSession _session;
        private readonly IPictureStore _pictureStore;
        private readonly IImageRecognitionService _imageRecognition;
        private readonly ILogger _logger;

        public SuggestPictureTagsQueryHandler(IStatelessSession session, IPictureStore pictureStore, IImageRecognitionService imageRecognition, ILogger logger)
        {
            _session = session;
            _pictureStore = pictureStore;
            _imageRecognition = imageRecognition;
            _logger = logger.ForContext<SuggestPictureTagsQuery>();
        }

        public async Task<Result<string[]>> Handle(SuggestPictureTagsQuery request, CancellationToken cancellationToken)
        {
            // A newly chosen picture takes precedence: it is the one the post is about to show.
            if (request.NewPictureRawBytes is not null)
            {
                return await SuggestTagsForNewPicture(request.NewPictureRawBytes);
            }

            // Without a new picture, the edit page asks for the tags of the picture already stored with the post.
            if (request.PostID is not null)
            {
                return await SuggestTagsForPictureOfPost(request.PostID, request.CurrentUserID, cancellationToken);
            }

            return Result.Failure<string[]>(PostErrors.PictureMissing);
        }

        private async Task<Result<string[]>> SuggestTagsForNewPicture(byte[] newPictureRawBytes)
        {
            // An empty upload is a missing picture, not a reason to fall back to the stored one.
            if (newPictureRawBytes.Length == 0)
            {
                return Result.Failure<string[]>(PostErrors.PictureMissing);
            }

            using var newPicture = new MemoryStream(newPictureRawBytes);
            return await _imageRecognition.AutoTagImage(newPicture);
        }

        private async Task<Result<string[]>> SuggestTagsForPictureOfPost(PostId postID, Guid currentUserID, CancellationToken cancellationToken)
        {
            var picture = await PictureOfPostEditableBy(postID, currentUserID, cancellationToken);
            if (picture.IsFailure)
            {
                return Result.Failure<string[]>(picture.Error);
            }

            await using var storedPicture = await _pictureStore.OpenReadAsync(PictureContainers.Posts, picture.Value, cancellationToken);
            if (storedPicture is null)
            {
                return Result.Failure<string[]>(PostErrors.PictureNotFound);
            }

            return await _imageRecognition.AutoTagImage(storedPicture);
        }

        private async Task<Result<PictureId>> PictureOfPostEditableBy(PostId postID, Guid currentUserID, CancellationToken cancellationToken)
        {
            using (_session.BeginTransaction())
            {
                var post = await _session.GetAsync<Post>(postID, cancellationToken);
                if (post is null)
                {
                    return Result.Failure<PictureId>(PostErrors.NotFound(postID));
                }

                if (!post.CanBeEditedBy(currentUserID))
                {
                    _logger.Warning("User {UserID} asked for tags for the picture of post {PostID} but is not allowed to edit it.",
                        currentUserID,
                        postID);
                    return Result.Failure<PictureId>(PostErrors.EditNotAllowed);
                }

                return Result.Ok(post.Picture);
            }
        }
    }
}
