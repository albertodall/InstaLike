using InstaLike.Core.SharedKernel;

namespace InstaLike.Core.Posts
{
    /// <summary>
    /// Represents validated text for a comment.
    /// </summary>
    /// <remarks>
    /// Comment text is trimmed and must contain at most 200 characters.
    /// </remarks>
    public sealed class CommentText : ValueObject<string>
    {
        internal const int CommentTextMaxLength = 200;

        private CommentText() : base(string.Empty) { }

        private CommentText(string text) : base(text) { }

        public static Result<CommentText> Create(string text)
        {
            var trimmedText = (text ?? string.Empty).Trim();

            if (trimmedText.Length == 0)
            {
                return Result.Failure<CommentText>(PostErrors.CommentTextEmpty);
            }

            if (trimmedText.Length > CommentTextMaxLength)
            {
                return Result.Failure<CommentText>(PostErrors.CommentTextTooLong);
            }

            return Result.Ok(new CommentText(trimmedText));
        }

        public static explicit operator CommentText(string text)
        {
            return Create(text).Value;
        }
    }
}
