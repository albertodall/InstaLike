namespace InstaLike.Core.Domain
{
    public abstract class EntityBase<TId> : IEntity<TId>
    {
        private const int HashMultiplier = 29;

        private int? cachedHashCode;

        public virtual TId ID { get; protected set; }

        public virtual bool IsTransient()
        {
            return ID == null || ID.Equals(default(TId));
        }

        public override bool Equals(object obj)
        {
            var other = obj as IEntity<TId>;

            if (other == null) return false;

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            if (IsTransient() && other.IsTransient())
            {
                return ReferenceEquals(this, other);
            }

            return other.ID.Equals(ID);
        }

        public override int GetHashCode()
        {
            if (cachedHashCode.HasValue)
            {
                return cachedHashCode.Value;
            }

            if (IsTransient())
            {
                cachedHashCode = typeof(IEntity<TId>).GetHashCode();
            }
            else
            {
                cachedHashCode = (HashMultiplier * GetType().GetHashCode()) ^ ID.GetHashCode();
            }

            return cachedHashCode.Value;
        }

        public static bool operator ==(EntityBase<TId> left, EntityBase<TId> right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(EntityBase<TId> left, EntityBase<TId> right)
        {
            return !Equals(left, right);
        }
    }
}
