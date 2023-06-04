using System;
using System.Runtime.CompilerServices;
using NHibernate.Proxy;

namespace InstaLike.Core.Domain
{
    public abstract class EntityBase<TId> : IEquatable<EntityBase<TId>>
        where TId : notnull
    {
        private const int HashMultiplier = 29;

#pragma warning disable CS8618
        protected EntityBase() { }
#pragma warning restore CS8618

        protected EntityBase(TId id)
            : this()
        {
            ID = id;
        }

        public virtual TId ID { get; private init; }

        public override bool Equals(object? obj)
        {
            if (obj is null)
            {
                return false;
            }

            if (obj is not EntityBase<TId> other)
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            if (GetUnproxiedType() != other.GetUnproxiedType())
            {
                return false;
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
            
            return (HashMultiplier * GetUnproxiedType().GetHashCode()) ^ ID.GetHashCode();
        }

        public virtual bool Equals(EntityBase<TId>? other)
        {
            if (other is null)
            {
                return false;
            }

            if (GetUnproxiedType() != other.GetUnproxiedType())
            {
                return false;
            }

            if (IsTransient() && other.IsTransient())
            {
                return ReferenceEquals(this, other);
            }

            return ID.Equals(other.ID);
        }

        public static bool operator ==(EntityBase<TId> left, EntityBase<TId> right)
        {
            return left is not null && right is not null && left.Equals(right);
        }

        public static bool operator !=(EntityBase<TId> left, EntityBase<TId> right)
        {
            return !(left == right);
        }

        private bool IsTransient()
        {
            return ID == null || ID.Equals(default(TId));
        }

        /// <remarks>
        /// Small violation of Separation of Concerns here...
        /// </remarks>
        private Type GetUnproxiedType()
        {
            return GetType().Name.Contains("ProxyForFieldInterceptor")
                ? GetType().BaseType!
                : NHibernateProxyHelper.GetClassWithoutInitializingProxy(this);
        }
    }
}
