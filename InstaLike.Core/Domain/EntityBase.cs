using System.Runtime.CompilerServices;

namespace InstaLike.Core.Domain
{
    public abstract class EntityBase<TId> : IEntity<TId>
    {
        private const int HashMultiplier = 29;

        public virtual TId ID { get; }

        protected EntityBase() { }

        protected EntityBase(TId id)
        {
            ID = id;
        }

        public virtual bool IsTransient()
        {
            return ID == null || ID.Equals(default(TId));
        }

        public override bool Equals(object obj)
        {
            if (!(obj is IEntity<TId> other)) return false;

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            if (IsTransient() && other.IsTransient())
            {
                return ReferenceEquals(this, other);
            }

            return ID.Equals(other.ID);
        }

        public override int GetHashCode()
        {
            if (IsTransient())
            {
                return RuntimeHelpers.GetHashCode(this);
            }
            
            return (HashMultiplier * GetType().GetHashCode()) ^ ID.GetHashCode();
        }

        public static bool operator ==(EntityBase<TId> left, EntityBase<TId> right)
        {
            if (left is null && right is null)
            {
                return true;
            }

            if (left is null || right is null)
            {
                return false;
            }

            return left.Equals(right);
        }

        public static bool operator !=(EntityBase<TId> left, EntityBase<TId> right)
        {
            return !(left == right);
        }
    }
}
