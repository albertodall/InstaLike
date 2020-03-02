using System;
using System.Collections.Generic;
using System.Linq;

namespace InstaLike.Web.Services
{
    internal class SequentialGuidGenerator : ISequentialGuidGenerator
    {
        private const int NumberOfGuidBytes = 10;
        private const int NumberOfSequenceBytes = 6;

        private static readonly DateTime BaseDate = new DateTime(1900, 1, 1);

        public Guid GetNextId()
        {
            var sequenceBytes = GetSequenceBytes(DateTime.UtcNow);
            var guidBytes = GetGuidBytes();
            var totalBytes = guidBytes.Concat(sequenceBytes).ToArray();

            return new Guid(totalBytes);
        }

        private static IEnumerable<byte> GetGuidBytes()
        {
            return Guid.NewGuid().ToByteArray().Take(NumberOfGuidBytes).ToArray();
        }

        private static IEnumerable<byte> GetSequenceBytes(DateTime value)
        {
            var ticksUntilNow = value.Ticks - BaseDate.Ticks;
            var sequenceBytes = BitConverter.GetBytes(ticksUntilNow);
            var sequenceBytesLongEnough = sequenceBytes.Concat(new byte[NumberOfSequenceBytes]);
            return sequenceBytesLongEnough.Take(NumberOfSequenceBytes).Reverse();
        }
    }
}
