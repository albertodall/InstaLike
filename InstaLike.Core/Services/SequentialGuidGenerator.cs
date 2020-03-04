using System;

namespace InstaLike.Core.Services
{
    /// <summary>
    /// Generates Sequential GUIDs.
    /// </summary>
    /// <remarks>
    /// Replaces the most significant 8 bytes of the GUID (according to SQL Server ordering) 
    /// with the current UTC-timestamp.
    /// </remarks>
    public class SequentialGuidGenerator : ISequentialIdGenerator<Guid>
    {
        private const byte NumberOfGuidBytes = 10;
        private const byte NumberOfSequenceBytes = 6;

        private static long _lastGenerationTicks = -1;
        private static readonly object SequenceGeneratorLocker = new object();

        public Guid GetNextId()
        {
            var ticks = DateTime.UtcNow.Ticks;

            lock (SequenceGeneratorLocker)
            {
                if (ticks <= _lastGenerationTicks)
                {
                    ticks = _lastGenerationTicks + 1;
                }

                _lastGenerationTicks = ticks;
            }

            var ticksBytes = BitConverter.GetBytes(ticks);

            Array.Reverse(ticksBytes);

            var newGuidBytes = Guid.NewGuid().ToByteArray();

            Array.Copy(ticksBytes, 0, newGuidBytes, NumberOfGuidBytes, NumberOfSequenceBytes);
            Array.Copy(ticksBytes, 6, newGuidBytes, 8, 2);

            return new Guid(newGuidBytes);
        }
    }
}
