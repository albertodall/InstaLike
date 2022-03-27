using System.Collections.Generic;
using CSharpFunctionalExtensions;

namespace InstaLike.Core.Domain
{
    public class CommentText : SimpleValueObject<string>
    {
        // public string Value { get; }

        private CommentText() : base(string.Empty) { }

        private CommentText(string text) : base(text) { }

        public static Result<CommentText> Create(string text)
        {
            var trimmedText = (text ?? string.Empty).Trim();

            if (trimmedText.Length == 0)
            {
                return Result.Failure<CommentText>("Text for a comment should not be empty.");
            }

            if (trimmedText.Length > 200)
            {
                return Result.Failure<CommentText>("Text for a comment should not be longer than 200 chars.");
            }

            return Result.Success(new CommentText(trimmedText));
        }

        public static implicit operator string(CommentText text)
        {
            return text.Value;
        }

        public static explicit operator CommentText(string text)
        {
            return Create(text).Value;
        }

        public override string ToString()
        {
            return Value;
        }
    }
}