using System;
using System.Collections.Generic;
using System.Text;
using CSharpFunctionalExtensions;

namespace InstaLike.Core.Domain
{
    public class Picture : ValueObject
    {
        private const string Default_Profile_Picture_Base64 = "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";
        private const string Empty_Picture_Base64 = "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAAYdEVYdFNvZnR3YXJlAHBhaW50Lm5ldCA0LjEuNv1OCegAAAANSURBVBhXY/j//z8DAAj8Av6IXwbgAAAAAElFTkSuQmCC";

        public virtual Guid Identifier { get; }

        public virtual byte[] RawBytes { get; }

        public static Picture DefaultProfilePicture => 
            Create(Convert.FromBase64String(Default_Profile_Picture_Base64))
            .Value;

        public static Picture EmptyPicture =>
            Create(Convert.FromBase64String(Empty_Picture_Base64))
            .Value;

        protected Picture()
        { }

        private Picture(byte[] rawBytes)
        {
            RawBytes = rawBytes;
        }

        public static Result<Picture> Create(byte[] rawBytes)
        {
            if (rawBytes.Length == 0)
            {
                return Result.Fail<Picture>("Unable to read picture data");
            }

            return Result.Ok(new Picture(rawBytes));
        }

        public static explicit operator Picture(byte[] bytes)
        {
            return Create(bytes).Value;
        }

        public static explicit operator Picture(string base64)
        {
            return Create(Encoding.ASCII.GetBytes(base64)).Value;
        }

        public static implicit operator byte[](Picture picture)
        {
            return picture.RawBytes;
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return RawBytes;
        }
    }
}