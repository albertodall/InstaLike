using CSharpFunctionalExtensions;

namespace InstaLike.Core.Domain
{
    public class PostText : SimpleValueObject<string>
    {
        private PostText() : base(string.Empty) { }

        private PostText(string text) : base(text) { }

        public static Result<PostText> Create(string text)
        {
            var trimmedText = (text ?? string.Empty).Trim();

            if (trimmedText.Length == 0)
            {
                return Result.Failure<PostText>("Text for a post should not be empty");
            }

            if (trimmedText.Length > 500)
            {
                return Result.Failure<PostText>("Text for a post should not be longer than 500 chars");
            }

            return Result.Success(new PostText(trimmedText));
        }

        public static implicit operator string(PostText text)
        {
            return text.Value;
        }

        public static explicit operator PostText(string text)
        {
            return Create(text).Value;
        }
    }
}