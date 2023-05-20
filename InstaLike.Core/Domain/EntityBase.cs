using System;
using System.Runtime.CompilerServices;
using NHibernate.Proxy;

namespace InstaLike.Core.Domain
{
    public abstract class EntityBase<TId> 
        where TId : notnull
    {
        private const int HashMultiplier = 29;

        public virtual TId ID { get; }

#pragma warning disable CS8618
        protected EntityBase() { }
#pragma warning restore CS8618

        protected EntityBase(TId id)
            : this()
        {
            ID = id;
        }

        private bool IsTransient()
        {
            return ID == null || ID.Equals(default(TId));
        }

        public override bool Equals(object? obj)
        {
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

        /// <remarks>
        /// Small violation of Separation of Concerns here...
        /// </remarks>
        private Type GetUnproxiedType()
        {
            return GetType().Name.Contains("ProxyForFieldInterceptor")
                ? GetType().BaseType!
                : NHibernateProxyHelper.GetClassWithoutInitializingProxy(this);
        }

        public override int GetHashCode()
        {
            if (IsTransient())
            {
                return RuntimeHelpers.GetHashCode(this);
            }
            
            return (HashMultiplier * GetUnproxiedType().GetHashCode()) ^ ID.GetHashCode();
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
