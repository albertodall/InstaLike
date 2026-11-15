using System.Collections.Generic;
using System.Linq;

namespace InstaLike.Core.SharedKernel
{
    /// <summary>
    /// Represents a value object in the domain, equal to another one of the same type with the same components.
    /// </summary>
    public abstract class ValueObject
    {
        protected abstract IEnumerable<object> GetEqualityComponents();

        public override bool Equals(object? obj)
        {
            if (obj is null)
            {
                return false;
            }

            if (GetType() != obj.GetType())
            {
                return false;
            }

            var valueObject = (ValueObject)obj;

            return GetEqualityComponents().SequenceEqual(valueObject.GetEqualityComponents());
        }

        public override int GetHashCode()
        {
            return GetEqualityComponents()
                .Aggregate(1, (current, obj) =>
                {
                    unchecked
                    {
                        return current * 23 + (obj?.GetHashCode() ?? 0);
                    }
                });
        }

        public static bool operator ==(ValueObject? a, ValueObject? b)
        {
            if (a is null && b is null)
            {
                return true;
            }

            if (a is null || b is null)
            {
                return false;
            }

            return a.Equals(b);
        }

        public static bool operator !=(ValueObject? a, ValueObject? b) => !(a == b);
    }

    /// <summary>
    /// Represents a value object that wraps a single value.
    /// </summary>
    public abstract class ValueObject<T> : ValueObject
        where T : notnull
    {
        public T Value { get; }

        protected ValueObject(T value)
        {
            Value = value;
        }

        public override string ToString() => Value.ToString() ?? string.Empty;

        public static implicit operator T(ValueObject<T> valueObject) => valueObject.Value;

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }
    }
}
