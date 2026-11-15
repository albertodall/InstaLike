using InstaLike.Core.SharedKernel;

namespace InstaLike.Core.Posts
{
    public static class PostErrors
    {
        public static Error NotFound(PostId postID) =>
            Error.NotFound("Post.NotFound", $"Post {postID} does not exist.");

        public static readonly Error PictureMissing =
            Error.Validation("Picture.Missing", "Please select a picture to share together with your post.");

        public static readonly Error TextEmpty =
            Error.Validation("PostText.Empty", "Text for a post should not be empty");

        public static readonly Error TextTooLong =
            Error.Validation("PostText.TooLong", $"Text for a post should not be longer than {PostText.PostTextMaxLength} chars");

        public static readonly Error CommentTextEmpty =
            Error.Validation("CommentText.Empty", "Text for a comment should not be empty.");

        public static readonly Error CommentTextTooLong =
            Error.Validation("CommentText.TooLong", $"Text for a comment should not be longer than {CommentText.CommentTextMaxLength} chars.");

        public static readonly Error OwnPostLiked =
            Error.Failure("Post.OwnPostLiked", "You cannot like your own posts.");

        public static readonly Error AlreadyLiked =
            Error.Conflict("Post.AlreadyLiked", "You have already liked this post.");

        public static readonly Error NotLiked =
            Error.Failure("Post.NotLiked", "You have not liked this post.");
    }
}
