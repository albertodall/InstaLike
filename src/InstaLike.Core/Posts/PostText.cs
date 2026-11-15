using InstaLike.Core.SharedKernel;

namespace InstaLike.Core.Posts
{
    /// <summary>
    /// Represents non-empty, trimmed text for a post, limited to 500 characters.
    /// </summary>
    public sealed class PostText : ValueObject<string>
    {
        internal const int PostTextMaxLength = 500;

        private PostText() : base(string.Empty) { }

        private PostText(string text) : base(text) { }

        public static Result<PostText> Create(string text)
        {
            var trimmedText = (text ?? string.Empty).Trim();

            if (trimmedText.Length == 0)
            {
                return Result.Failure<PostText>(PostErrors.TextEmpty);
            }

            if (trimmedText.Length > PostTextMaxLength)
            {
                return Result.Failure<PostText>(PostErrors.TextTooLong);
            }

            return Result.Ok(new PostText(trimmedText));
        }

        public static explicit operator PostText(string text)
        {
            return Create(text).Value;
        }
    }
}
