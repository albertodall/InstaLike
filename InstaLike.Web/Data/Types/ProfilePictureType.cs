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
        private const string Profile_Pictures_Container_Name = "profiles";

        public override object NullSafeGet(DbDataReader dr, string[] names, ISessionImplementor session, object owner)
        {
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
                result = provider.Value.LoadPictureAsync($"{pictureGuid.ToString().ToLowerInvariant()}.jpg", Profile_Pictures_Container_Name).Result;
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

            Maybe<IExternalStorageProvider> provider = GetExternalStorageProvider(session);
            if (provider.HasNoValue)
            {
                // No external storage provider configured, save the picture in the database
                NHibernateUtil.BinaryBlob.NullSafeSet(cmd, picture.RawBytes, index, session);
            }
            else
            {
                // Save the picture using the configured external storage provider.
                provider.Value.SavePictureAsync(picture, Profile_Pictures_Container_Name).GetAwaiter().GetResult();
                NHibernateUtil.BinaryBlob.NullSafeSet(cmd, Array.Empty<byte>(), index, session);
            }

            // Guid reference has always to be saved in the database.
            NHibernateUtil.Guid.NullSafeSet(cmd, picture.Identifier, ++index, session);
        }
    }
}