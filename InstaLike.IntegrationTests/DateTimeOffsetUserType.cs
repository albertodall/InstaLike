using System;
using System.Data;
using System.Data.Common;
using NHibernate;
using NHibernate.Engine;
using NHibernate.SqlTypes;
using NHibernate.Type;
using NHibernate.UserTypes;

namespace InstaLike.IntegrationTests
{
    /// <summary>
    /// User Type for managing DateTimeOffset fields.
    /// SQLite doesn't have a DateTimeOffset type, so this user type splits the original DateTimeOffset value
    /// into two fields:
    ///     - Ticks: to store the datetime part.
    ///     - Offset: to store the original offset.
    /// </summary>
    /// <remarks>
    /// This type has to be injected into the NHibernate configuration by convention, because we don't want
    /// to change the types defined in the original mappings.
    /// </remarks>
    internal class DateTimeOffsetUserType : ICompositeUserType
    {
        public string[] PropertyNames => new[] { "Ticks", "Offset" };

        public IType[] PropertyTypes => new IType[] { NHibernateUtil.Ticks, NHibernateUtil.TimeSpan };

        public Type ReturnedClass => typeof(DateTimeOffset);

        public bool IsMutable => false;

        public object Assemble(object cached, ISessionImplementor session, object owner) => cached;

        public object DeepCopy(object value) => value;

        public object Disassemble(object value, ISessionImplementor session) => value;

        public new bool Equals(object x, object y)
        {
            if (x is null && y is null)
                return true;

            if (x is null || y is null)
                return false;

            return x.Equals(y);
        }

        public int GetHashCode(object x)
        {
            return x.GetHashCode();
        }

        public object GetPropertyValue(object component, int property)
        {
            throw new NotImplementedException();
        }

        public object NullSafeGet(DbDataReader dr, string[] names, ISessionImplementor session, object owner)
        {
            if (dr.IsDBNull(dr.GetOrdinal(names[0])))
            {
                return null;
            }

            var dateTime = (DateTime)NHibernateUtil.Ticks.NullSafeGet(dr, names[0], session, owner);
            var offset = (TimeSpan)NHibernateUtil.TimeSpan.NullSafeGet(dr, names[1], session, owner);

            return new DateTimeOffset(dateTime, offset);
        }

        public void NullSafeSet(DbCommand cmd, object value, int index, bool[] settable, ISessionImplementor session)
        {
            object utcTicks = null;
            object offset = null;

            if (value != null)
            {
                utcTicks = ((DateTimeOffset)value).DateTime;
                offset = ((DateTimeOffset)value).Offset;
            }

            NHibernateUtil.Ticks.NullSafeSet(cmd, utcTicks, index++, session);
            NHibernateUtil.TimeSpan.NullSafeSet(cmd, offset, index, session);
        }

        public object Replace(object original, object target, ISessionImplementor session, object owner) => original;

        public void SetPropertyValue(object component, int property, object value)
        {
            throw new NotImplementedException();
        }
    }
}