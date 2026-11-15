using System;
using System.Data.Common;
using NHibernate;
using NHibernate.Engine;
using NHibernate.SqlTypes;
using NHibernate.UserTypes;

namespace InstaLike.IntegrationTests
{
    /// <summary>
    /// User Type for managing DateTimeOffset fields.
    /// SQLite doesn't have a DateTimeOffset type, so, to address that, this user type stores the instant as UTC ticks
    /// in a single column. Comparing and sorting that column works like SQL Server's datetimeoffset, which compares instants.
    /// The original offset is not kept: dates are read back in UTC.
    /// </summary>
    /// <remarks>
    /// This type has to be injected into the NHibernate configuration by convention, because we don't want
    /// to change the types defined in the original mappings (<see cref="DateTimeOffsetTypeConvention"/>).
    /// </remarks>
    internal sealed class DateTimeOffsetUserType : IUserType
    {
        public SqlType[] SqlTypes => new[] { SqlTypeFactory.Int64 };

        public Type ReturnedType => typeof(DateTimeOffset);

        public bool IsMutable => false;

        public object Assemble(object cached, object owner) => cached;

        public object DeepCopy(object value) => value;

        public object Disassemble(object value) => value;

        public object Replace(object original, object target, object owner) => original;

        public new bool Equals(object x, object y)
        {
            if (x is null && y is null)
            {
                return true;
            }

            if (x is null || y is null)
            {
                return false;
            }

            return x.Equals(y);
        }

        public int GetHashCode(object x)
        {
            return x.GetHashCode();
        }

        public object? NullSafeGet(DbDataReader rs, string[] names, ISessionImplementor session, object owner)
        {
            var utcTicks = NHibernateUtil.Int64.NullSafeGet(rs, names[0], session);
            if (utcTicks is null)
            {
                return null;
            }

            return new DateTimeOffset((long)utcTicks, TimeSpan.Zero);
        }

        public void NullSafeSet(DbCommand cmd, object value, int index, ISessionImplementor session)
        {
            object? utcTicks = null;
            if (value != null)
            {
                utcTicks = ((DateTimeOffset)value).UtcTicks;
            }

            NHibernateUtil.Int64.NullSafeSet(cmd, utcTicks, index, session);
        }
    }
}
