using System;
using System.Collections.Generic;
using CSharpFunctionalExtensions;

namespace InstaLike.Core.Domain
{
    public class Picture : ValueObject
    {
        public virtual Guid Identifier { get; }
        public virtual string FileName { get; }
        public virtual byte[] RawBytes { get; }

        protected Picture()
        { }

        private Picture(Guid identifier, byte[] rawBytes)
        {
            Identifier = identifier;
            RawBytes = rawBytes;
        }

        public static Result<Picture> Create(Guid identifier, byte[] rawBytes)
        {
            if (rawBytes.Length == 0)
            {
                return Result.Fail<Picture>("Unable to read picture data");
            }

            return Result.Ok(new Picture(identifier, rawBytes));
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Identifier;
            yield return FileName;
            yield return RawBytes;
        }
    }
}