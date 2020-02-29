using System;
using System.Data.Common;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using InstaLike.Core.Domain;
using InstaLike.Web.Services;
using NHibernate;
using NHibernate.Engine;
using NHibernate.Type;
using NHibernate.UserTypes;

namespace InstaLike.Web.Data.Types
{
    internal abstract class ExternalStoragePictureType : ICompositeUserType
    {
        public string[] PropertyNames => new[] { "RawBytes", "Identifier" };

        public IType[] PropertyTypes => new IType[] { NHibernateUtil.BinaryBlob, NHibernateUtil.Guid };

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
            var picture = component as Picture;

            if (picture == null)
            {
                throw new ArgumentNullException($"{nameof(component)} is not a valid picture");
            }

            if (property == 0)
            {
                return picture.RawBytes;
            }
            return picture.Identifier;
        }

        public abstract object NullSafeGet(DbDataReader dr, string[] names, ISessionImplementor session, object owner);

        public abstract void NullSafeSet(DbCommand cmd, object value, int index, bool[] settable, ISessionImplementor session);

        public object Replace(object original, object target, ISessionImplementor session, object owner) => original;

        public void SetPropertyValue(object component, int property, object value)
        {
            throw new InvalidOperationException($"{nameof(Picture)} is an immutable object. SetPropertyValue is not supported.");
        }

        protected static Picture LoadPictureFromConfiguredProvider(DbDataReader dr, string guidFieldName, string pictureBytesFieldName, ISessionImplementor session, string containerName)
        {
            Picture result;

            // Read the picture Guid in the database, actual link between database and external storage.
            var pictureGuid = dr.GetFieldValue<Guid>(dr.GetOrdinal(guidFieldName));
            var provider = GetExternalStorageProvider(session);
            if (provider.HasNoValue)
            {
                // No external provider configured, read everything from database.
                var pictureBytes = dr.GetFieldValue<byte[]>(dr.GetOrdinal(pictureBytesFieldName));
                result = Picture.Create(pictureBytes, pictureGuid).Value;
            }
            else
            {
                // Read the picture using the configured external provider
                result = Task.Run(() =>
                    provider.Value.LoadPictureAsync($"{pictureGuid.ToString().ToLowerInvariant()}.jpg", containerName)
                ).Result;
            }

            return result;
        }

        protected static void SavePictureToConfiguredProvider(DbCommand cmd, Picture picture, int index, ISessionImplementor session, string containerName)
        {
            var provider = GetExternalStorageProvider(session);
            if (provider.HasNoValue)
            {
                // No external storage provider configured, save the picture in the database
                NHibernateUtil.BinaryBlob.NullSafeSet(cmd, picture.RawBytes, index, session);
            }
            else
            {
                // Save the picture using the configured external storage provider.
                Task.Run(
                    () => provider.Value.SavePictureAsync(picture, containerName)
                ).Wait();
                NHibernateUtil.BinaryBlob.NullSafeSet(cmd, Array.Empty<byte>(), index, session);
            }

            // Guid reference must always be saved in the database.
            NHibernateUtil.Guid.NullSafeSet(cmd, picture.Identifier, ++index, session);
        }

        private static Maybe<IExternalStorageProvider> GetExternalStorageProvider(ISessionImplementor session)
        {
            if (session.Connection == null)
            {
                throw new NullReferenceException($"{nameof(ExternalStoragePictureType)} requires an open connection.");
            }

            if (session.Connection is IExternalStorageProvider connection)
            {
                return Maybe<IExternalStorageProvider>.From(connection);
            }

            return Maybe<IExternalStorageProvider>.None;
        }
    }
}