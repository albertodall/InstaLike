using System;
using System.Data.Common;
using CSharpFunctionalExtensions;
using InstaLike.Core.Domain;
using InstaLike.Web.Services;
using NHibernate;
using NHibernate.Engine;

namespace InstaLike.Web.Data.Types
{
    internal class ProfilePictureType : ExternalStoragePictureType
    {
        public override object NullSafeGet(DbDataReader dr, string[] names, ISessionImplementor session, object owner)
        {
            // This is only a PoC.
            Picture result;

            // Read the picture Guid in the database, to create the link between the database and the external storage.
            var pictureGuid = dr.GetFieldValue<Guid>(dr.GetOrdinal(names[1]));
            Maybe<IExternalStorageProvider> provider = GetExternalStorageProvider(session);
            if (provider.HasNoValue)
            {
                // No external provider configured, read everything from the database.
                var pictureBytes = dr.GetFieldValue<byte[]>(dr.GetOrdinal(names[0]));
                result = Picture.Create(pictureBytes, pictureGuid).Value;
            }
            else
            {
                // Read the picture using the configured external provider
                // TODO: the blob container name should not be there
                result = provider.Value.LoadPictureAsync($"{pictureGuid.ToString().ToLowerInvariant()}.jpg", "profiles").Result;
            }

            return result;
        }

        public override void NullSafeSet(DbCommand cmd, object value, int index, bool[] settable, ISessionImplementor session)
        {
            var picture = value as Picture;
            if (picture == null)
            {
                throw new ArgumentNullException(nameof(value), $"Specified value is not a {nameof(Picture)}");
            }

            // Guid reference has always to be saved in the database.
            NHibernateUtil.Guid.NullSafeSet(cmd, picture.Identifier, index + 1, session);

            Maybe<IExternalStorageProvider> provider = GetExternalStorageProvider(session);
            if (provider.HasNoValue)
            {
                // No external storage provider configured, save the picture in the database
                NHibernateUtil.BinaryBlob.NullSafeSet(cmd, picture.RawBytes, index, session);
            }
            else
            {
                // Save the picture using the configured external storage provider.
                // TODO: the blob container name should not be there
                provider.Value.SavePictureAsync(picture, "profiles").GetAwaiter().GetResult();
            }
        }
    }
}