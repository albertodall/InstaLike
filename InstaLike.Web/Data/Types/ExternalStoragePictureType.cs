using System;
using System.Data.Common;
using InstaLike.Core.Domain;
using InstaLike.Web.Infrastructure;
using InstaLike.Web.Services;
using NHibernate;
using NHibernate.Engine;
using NHibernate.SqlTypes;
using NHibernate.UserTypes;

namespace InstaLike.Web.Data.Types;

/// <summary>
/// Maps a <see cref="Picture"/> stored in Azure Blob Storage.
/// The database only keeps the picture Guid, which is the link to the blob.
/// </summary>
internal abstract class ExternalStoragePictureType : IUserType
{
    protected ExternalStoragePictureType(string containerName)
    {
        ContainerName = containerName;
    }

    public SqlType[] SqlTypes => [SqlTypeFactory.Guid];

    public Type ReturnedType => typeof(Picture);

    public bool IsMutable => false;

    private string ContainerName { get; }

    public object Assemble(object cached, object owner) => cached;

    public object DeepCopy(object value) => value;

    public object Disassemble(object value) => value;

    public new bool Equals(object x, object y) => Object.Equals(x, y);

    public int GetHashCode(object x) => x.GetHashCode();

    public object Replace(object original, object target, object owner) => original;

    public object NullSafeGet(DbDataReader rs, string[] names, ISessionImplementor session, object owner)
    {
        if (NHibernateUtil.Guid.NullSafeGet(rs, names[0], session) is not Guid pictureGuid)
        {
            return Picture.MissingPicture;
        }

        return GetExternalStorageProvider(session).LoadPicture(pictureGuid, ContainerName);
    }

    public void NullSafeSet(DbCommand cmd, object value, int index, ISessionImplementor session)
    {
        if (value is not Picture picture)
        {
            throw new ArgumentException($"Specified value is not a {nameof(Picture)}", nameof(value));
        }

        GetExternalStorageProvider(session).SavePicture(picture, ContainerName);

        NHibernateUtil.Guid.NullSafeSet(cmd, picture.Identifier, index, session);
    }

    private static IExternalStorageProvider GetExternalStorageProvider(ISessionImplementor session)
    {
        return session.Connection is HybridStorageConnection connection
            ? connection.ExternalStorage
            : throw new InvalidOperationException(
                $"{nameof(ExternalStoragePictureType)} requires a {nameof(HybridStorageConnection)}. Check that the external storage is configured.");
    }
}
