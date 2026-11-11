using System;
using System.Data.Common;
using InstaLike.Core.SharedKernel;
using NHibernate;
using NHibernate.Engine;
using NHibernate.SqlTypes;
using NHibernate.UserTypes;

namespace InstaLike.Web.Data.Types;

internal sealed class PictureIdType : IUserType
{
    public SqlType[] SqlTypes => [SqlTypeFactory.Guid];

    public Type ReturnedType => typeof(PictureId);

    public bool IsMutable => false;

    public object Assemble(object cached, object owner) => cached;

    public object DeepCopy(object value) => value;

    public object Disassemble(object value) => value;

    public new bool Equals(object x, object y) => object.Equals(x, y);

    public int GetHashCode(object x) => x.GetHashCode();

    public object Replace(object original, object target, object owner) => original;

    public object? NullSafeGet(DbDataReader rs, string[] names, ISessionImplementor session, object owner) =>
        NHibernateUtil.Guid.NullSafeGet(rs, names[0], session) is Guid value ? new PictureId(value) : null;

    public void NullSafeSet(DbCommand cmd, object? value, int index, ISessionImplementor session) =>
        NHibernateUtil.Guid.NullSafeSet(cmd, (value as PictureId)?.Value, index, session);
}
