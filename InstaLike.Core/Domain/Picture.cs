using System;
using System.Collections.Generic;
using CSharpFunctionalExtensions;

namespace InstaLike.Core.Domain
{
    public class Picture : ValueObject
    {
        private const string DefaultProfilePictureBase64 = "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";
        private const string DefaultProfilePictureGuid = "00010000-0000-0000-0000-000000000000";

        private const string EmptyPictureBase64 = "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAAYdEVYdFNvZnR3YXJlAHBhaW50Lm5ldCA0LjEuNv1OCegAAAANSURBVBhXY/j//z8DAAj8Av6IXwbgAAAAAElFTkSuQmCC";
        private const string EmptyPictureGuid = "00000100-0000-0000-0000-000000000000";

        private const string MissingPictureBase64 = "iVBORw0KGgoAAAANSUhEUgAAAMgAAADICAMAAACahl6sAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAMAUExURQAAALe3t7i4uLm5ubq6uru7u7y8vL29vb6+vr+/v8DAwMHBwcLCwsPDw8TExMXFxcbGxsfHx8jIyMnJycrKysvLy8zMzM3Nzc7Ozs/Pz9DQ0NHR0dLS0tPT09TU1NXV1dbW1tfX19jY2NnZ2dra2tvb29zc3N3d3d7e3t/f3+Dg4OHh4eLi4uPj4+Tk5OXl5ebm5ufn5+jo6Onp6erq6uvr6+zs7O3t7e7u7u/v7/Dw8PHx8fT09PX19QAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAK/4yyIAAAEAdFJOU////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////wBT9wclAAAACXBIWXMAAA7DAAAOwwHHb6hkAAAMI0lEQVR4Xu2daXfiuBKGrzBbyNZZICuQNGHpQPZ0Orl35v//LK6NCy1WqbRYmeMzx8+XDjLIfm0tVaWS+z/rfwm1kKpRC6katZCqUQupGrWQqlELqRq1kKpRC6katZCqUQupGrWQqlELqRq1kKpRC7Hy+TAbDo72e51Ws9ne6R2enN8snuHYN/AtQp5/nnQaBnYH03f4WlSiC3kZ7jK4ZjP74zf4ejTiClmeJnCpVpL+Cn4Uh4hCnvtwjc70n+CnEYgm5NbYKyjaN/Dz0sQR8nUGFxbA4A9UUo4YQj5O4JoCOf6AisoQQYh319A5+YK6wikt5AquRYex5s6PwdXoJmV02T/cSZhxYGYXUF0wJYUsWnAlKiw5HN4jbf9jMTxIcDXNKXwnkHJCDuAqZFhyPP2E4yif02NUzF6p9lVGyASuQII1L5zsqZfLFqLlFo6GUEII8jhOPWY4bP48gGMBBAt5aMLJOS3vGzrpFB9L8wEOeRMqZAhn5nR/wREvlr2ilGs44kugkCM47ZZOkIyM1Q5UseUIDngSJqRw8mQC5UHMC410B8r9CBHy0YZT5rABlAdzobavdojJEiDkRfU5uq9QXoJ39RE3A6r0F/IMZ8thl1BckkvloST+zr23kCc4V04rmmv0qPYUbyW+QtTnUWIC09mFSnN8nXpPIb/l/lHeZFVR3Jqmp7/lKUQxdkvaqzqKS9CCQkf8hHThJBseoTAej1Bzjt984iXkEE6RkcQPs6njiOcc7yNEfvK+TdgBdRzJGMIRFzyErKSh/h/R0WAetrCHEGmg/4Z2hehI7xccdMBdiOxHRYwQAqiORmMfDttxFvITqk5hcyiLx6spZvwTvmDFWQhUnHEFRfEw6mg0XCMSrkL2od6U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";
        private const string MissingPictureGuid = "00000001-0000-0000-0000-000000000000";

        public virtual Guid Identifier { get; }

        public virtual byte[] RawBytes { get; }

        public virtual long Size => RawBytes.LongLength;

        public static Picture DefaultProfilePicture =>
            Create(Convert.FromBase64String(DefaultProfilePictureBase64), new Guid(DefaultProfilePictureGuid))
            .Value;

        public static Picture EmptyPicture =>
            Create(Convert.FromBase64String(EmptyPictureBase64), new Guid(EmptyPictureGuid))
            .Value;

        public static Picture MissingPicture =>
            Create(Convert.FromBase64String(MissingPictureBase64), new Guid(MissingPictureGuid))
            .Value;

        protected Picture()
        { }

        private Picture(byte[] rawBytes, Guid identifier)
        {
            RawBytes = rawBytes;
            Identifier = identifier;
        }

        public static Result<Picture> Create(byte[] rawBytes, Guid identifier)
        {
            if (rawBytes.Length == 0)
            {
                return Result.Success(MissingPicture);
            }

            return Result.Success(new Picture(rawBytes, identifier));
        }

        public static Result<Picture> Create(byte[] rawBytes)
        {
            return Create(rawBytes, new Guid("00000000-0000-0000-0000-000000000000"));
        }

        public static explicit operator Picture(byte[] bytes)
        {
            return Create(bytes).Value;
        }

        public static explicit operator Picture(string base64)
        {
            return Create(Convert.FromBase64String(base64)).Value;
        }

        public static implicit operator byte[](Picture picture)
        {
            return picture.RawBytes;
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Convert.ToBase64String(RawBytes);
            yield return Identifier;
        }
    }
}