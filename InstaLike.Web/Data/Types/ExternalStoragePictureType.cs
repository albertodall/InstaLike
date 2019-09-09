using System;
using System.Data.Common;
using InstaLike.Core.Domain;
using InstaLike.Web.Infrastructure;
using InstaLike.Web.Services;
using NHibernate;
using NHibernate.Engine;
using NHibernate.Type;
using NHibernate.UserTypes;

namespace InstaLike.Web.Data.Types
{
    internal abstract class ExternalStoragePictureType : ICompositeUserType
    {
        public string[] PropertyNames => new string[2] { "RawBytes", "Identifier" };

        public IType[] PropertyTypes => new IType[2] { NHibernateUtil.BinaryBlob, NHibernateUtil.Guid };

        public Type ReturnedClass => typeof(Picture);

        public bool IsMutable => false;

        public object Assemble(object cached, ISessionImplementor session, object owner) => DeepCopy(cached);

        public object DeepCopy(object value)
        {
            var picture = value as Picture;
            if (picture == null)
            {
                return Picture.MissingPicture;
            }

            return Picture.Create(picture.RawBytes, picture.Identifier).Value;
        }

        public object Disassemble(object value, ISessionImplementor session) => DeepCopy(value);

        public new bool Equals(object x, object y) => x.Equals(y);

        public int GetHashCode(object x) => x.GetHashCode();

        public object GetPropertyValue(object component, int property)
        {
            Picture picture = component as Picture;
            if (property == 0)
            {
                return picture.RawBytes;
            }
            else
            {
                return picture.Identifier;
            }
        }

        public abstract object NullSafeGet(DbDataReader dr, string[] names, ISessionImplementor session, object owner);     

        public abstract void NullSafeSet(DbCommand cmd, object value, int index, bool[] settable, ISessionImplementor session);

        public object Replace(object original, object target, ISessionImplementor session, object owner) => original;

        public void SetPropertyValue(object component, int property, object value)
        {
            throw new InvalidOperationException($"{nameof(Picture)} is an immutable object. SetPropertyValue is not supported.");
        }

        protected static IExternalStorageProvider GetExternalStorageProvider(ISessionImplementor session)
        {
            if (session.Connection == null)
            {
                throw new NullReferenceException($"{nameof(ExternalStoragePictureType)} requires an open connection.");
            }

            if (!(session.Connection is IExternalStorageProvider connection))
            {
                throw new Exception(
                    $"{nameof(ExternalStoragePictureType)} requires a {nameof(IExternalStorageProvider)}." +
                    $"Make sure you use {nameof(ExternalStorageDriverConnectionProvider)} as your connection provider and specify a {nameof(IExternalStorageConnectionProvider)} in your NHibernate configuration.");
            }

            return connection;
        }
    }
}