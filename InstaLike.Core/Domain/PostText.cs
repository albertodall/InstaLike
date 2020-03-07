using System.Collections.Generic;
using CSharpFunctionalExtensions;

namespace InstaLike.Core.Domain
{
    public class PostText : ValueObject
    {
        public string Value { get; }

        protected PostText()
        { }

        private PostText(string text)
        {
            Value = text;
        }

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

        public override string ToString()
        {
            return Value;
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value.ToUpperInvariant();
        }
    }
}