using System;
using System.Collections.Generic;
using System.Linq;

namespace InstaLike.Core.Services
{
    /// <summary>
    /// Represents a <see cref="SequentialGuid"/>; it's a <see cref="Guid">GUID</see> but the most significant bytes are created sequentially.
    /// This type wraps a <see cref="Guid"/> because all <see cref="SequentialGuid">Sequential GUIDs</see> are <see cref="Guid">GUIDs</see>,
    /// but not all <see cref="Guid">GUIDs</see> are <see cref="SequentialGuid">Sequential GUIDs</see>.
    /// </summary>
    /// <remarks>
    /// Taken from <see cref="https://www.siepman.nl/blog/post/2015/06/20/SequentialGuid-Comb-Sql-Server-With-Creation-Date-Time-.aspx"/>
    /// </remarks>
    [Serializable]
    public struct SequentialGuid : IComparable<SequentialGuid>, IComparable<Guid>, IComparable
    {
        private const byte NumberOfGuidBytes = 10;
        private const byte NumberOfSequenceBytes = 6;
        private const short PermutationsOfAByte = 256;
        
        private static readonly long MaximumPermutations = (long)Math.Pow(PermutationsOfAByte, NumberOfSequenceBytes);
        private static readonly object SynchronizationObject = new();
        
        private static readonly DateTime SequencePeriodStart = new(1900, 1, 1, 0, 0, 0, DateTimeKind.Utc); // Start = 000000
        private static readonly DateTime SequencePeriodEnd = new(2500, 1, 1, 0, 0, 0, DateTimeKind.Utc); // End   = FFFFFF
        private static readonly int[] IndexOrderingHighLow = { 10, 11, 12, 13, 14, 15, 8, 9, 7, 6, 5, 4, 3, 2, 1, 0 };

        private static long _lastSequence;

        public Guid Value { get; }

        private SequentialGuid(Guid guid)
        {
            Value = guid;
        }

        public static SequentialGuid NewGuid()
        {
            return new SequentialGuid(GetGuidValue(DateTime.UtcNow));
        }

        public static TimeSpan TotalPeriod => SequencePeriodEnd - SequencePeriodStart;

        private static Guid GetGuidValue(DateTime value)
        {
            // Outside the range, use regular Guid
            if (value < SequencePeriodStart || value >= SequencePeriodEnd)
            {
                return Guid.NewGuid(); 
            }

            var sequence = GetCurrentSequence(value);
            lock (SynchronizationObject)
            {
                if (sequence <= _lastSequence)
                {
                    // Prevent double sequence on the same machine
                    sequence = _lastSequence + 1;
                }
                _lastSequence = sequence;
            }

            var sequenceBytes = GetSequenceBytes(sequence);
            var guidBytes = GetGuidBytes();
            return new Guid(guidBytes.Concat(sequenceBytes).ToArray());
        }

        private static long GetCurrentSequence(DateTime value)
        {
            var ticksUntilNow = value.Ticks - SequencePeriodStart.Ticks;
            var factor = (decimal)ticksUntilNow / TotalPeriod.Ticks;
            var resultDecimal = factor * MaximumPermutations;
            return (long)resultDecimal;
        }
        
        private static IEnumerable<byte> GetSequenceBytes(long sequence)
        {
            var sequenceBytes = BitConverter.GetBytes(sequence);
            var sequenceBytesLongEnough = sequenceBytes.Concat(new byte[NumberOfSequenceBytes]);
            return sequenceBytesLongEnough.Take(NumberOfSequenceBytes).Reverse();
        }

        private static IEnumerable<byte> GetGuidBytes()
        {
            return Guid.NewGuid().ToByteArray().Take(NumberOfGuidBytes);
        }

        public static bool operator <(SequentialGuid value1, SequentialGuid value2)
        {
            return value1.CompareTo(value2) < 0;
        }

        public static bool operator >(SequentialGuid value1, SequentialGuid value2)
        {
            return value1.CompareTo(value2) > 0;
        }

        public static bool operator <(Guid value1, SequentialGuid value2)
        {
            return value1.CompareTo(value2) < 0;
        }

        public static bool operator >(Guid value1, SequentialGuid value2)
        {
            return value1.CompareTo(value2) > 0;
        }

        public static bool operator <(SequentialGuid value1, Guid value2)
        {
            return value1.CompareTo(value2) < 0;
        }

        public static bool operator >(SequentialGuid value1, Guid value2)
        {
            return value1.CompareTo(value2) > 0;
        }

        public static bool operator <=(SequentialGuid value1, SequentialGuid value2)
        {
            return value1.CompareTo(value2) <= 0;
        }

        public static bool operator >=(SequentialGuid value1, SequentialGuid value2)
        {
            return value1.CompareTo(value2) >= 0;
        }

        public static bool operator <=(Guid value1, SequentialGuid value2)
        {
            return value1.CompareTo(value2) <= 0;
        }

        public static bool operator >=(Guid value1, SequentialGuid value2)
        {
            return value1.CompareTo(value2) >= 0;
        }

        public static bool operator <=(SequentialGuid value1, Guid value2)
        {
            return value1.CompareTo(value2) <= 0;
        }

        public static bool operator >=(SequentialGuid value1, Guid value2)
        {
            return value1.CompareTo(value2) >= 0;
        }

        public static bool operator ==(SequentialGuid value1, SequentialGuid value2)
        {
            return value1.CompareTo(value2) == 0;
        }

        public static bool operator !=(SequentialGuid value1, SequentialGuid value2)
        {
            return !(value1 == value2);
        }

        public static bool operator ==(Guid value1, SequentialGuid value2)
        {
            return value1.CompareTo(value2) == 0;
        }

        public static bool operator !=(Guid value1, SequentialGuid value2)
        {
            return !(value1 == value2);
        }

        public static bool operator ==(SequentialGuid value1, Guid value2)
        {
            return value1.CompareTo(value2) == 0;
        }

        public static bool operator !=(SequentialGuid value1, Guid value2)
        {
            return !(value1 == value2);
        }

        public int CompareTo(object? obj)
        {
            return obj switch
            {
                SequentialGuid seqGuid => CompareTo(seqGuid),
                Guid guid => CompareTo(guid),
                _ => throw new ArgumentException("Parameter is neither a Guid nor a SequentialGuid."),
            };
        }

        public int CompareTo(SequentialGuid other)
        {
            return CompareTo(other.Value);
        }

        public int CompareTo(Guid other)
        {
            return CompareInternal(Value, other);
        }

        private static int CompareInternal(Guid left, Guid right)
        {
            var leftBytes = left.ToByteArray();
            var rightBytes = right.ToByteArray();

            return IndexOrderingHighLow
                .Select(i => leftBytes[i].CompareTo(rightBytes[i]))
                .FirstOrDefault(r => r != 0);
        }

        public override bool Equals(object? obj)
        {
            if (obj is SequentialGuid || obj is Guid)
            {
                return CompareTo(obj) == 0;
            }

            return false;
        }

        public bool Equals(SequentialGuid other)
        {
            return CompareTo(other) == 0;
        }

        public bool Equals(Guid other)
        {
            return CompareTo(other) == 0;
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public static implicit operator Guid(SequentialGuid value)
        {
            return value.Value;
        }

        public static explicit operator SequentialGuid(Guid value)
        {
            return new SequentialGuid(value);
        }
    }
}
