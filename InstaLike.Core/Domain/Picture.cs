using System;
using System.Collections.Generic;
using CSharpFunctionalExtensions;
using InstaLike.Core.Properties;

namespace InstaLike.Core.Domain
{
    public class Picture : ValueObject
    { 
        private const string DefaultProfilePictureGuid = "00010000-0000-0000-0000-000000000000";
        private const string PicturePlaceholderGuid = "00000100-0000-0000-0000-000000000000";
        private const string MissingPictureGuid = "00000001-0000-0000-0000-000000000000";

        private static readonly string DefaultProfilePictureBase64 = Convert.ToBase64String(Resources.DefaultProfilePicture);
        private static readonly string PicturePlaceholderBase64 = Convert.ToBase64String(Resources.PicturePlaceholder);
        private static readonly string MissingPictureBase64 = Convert.ToBase64String(Resources.MissingPicture);

        public Guid Identifier { get; }

        public byte[] RawBytes { get; }

        public long Size => RawBytes.LongLength;

        public static Picture DefaultProfilePicture =>
            Create(Convert.FromBase64String(DefaultProfilePictureBase64), new Guid(DefaultProfilePictureGuid))
            .Value;

        public static Picture EmptyPicture =>
            Create(Convert.FromBase64String(PicturePlaceholderBase64), new Guid(PicturePlaceholderGuid))
            .Value;

        public static Picture MissingPicture =>
            Create(Convert.FromBase64String(MissingPictureBase64), new Guid(MissingPictureGuid))
            .Value;

        private Picture() { }

        private Picture(byte[] rawBytes, Guid identifier) : this()
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

        protected override IEnumerable<IComparable> GetEqualityComponents()
        {
            yield return Convert.ToBase64String(RawBytes);
            yield return Identifier;
        }
    }
}