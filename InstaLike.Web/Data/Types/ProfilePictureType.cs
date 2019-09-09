using System;
using System.Data.Common;
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
            var pictureGuid = dr.GetFieldValue<Guid>(dr.GetOrdinal(names[1])).ToString().ToLowerInvariant();

            IExternalStorageProvider provider = GetExternalStorageProvider(session);
            // TODO: the blob container name should not be there
            var picture = provider.LoadPictureAsync($"{pictureGuid}.jpg", "profiles").Result;

            return picture;
        }

        public override void NullSafeSet(DbCommand cmd, object value, int index, bool[] settable, ISessionImplementor session)
        {
            var picture = value as Picture;
            if (picture == null)
            {
                throw new ArgumentNullException(nameof(value), $"{nameof(Picture)} cannot be null");
            }

            if (picture != Picture.DefaultProfilePicture)
            {
                // NHibernateUtil.BinaryBlob.NullSafeSet(cmd, picture.RawBytes, index, session);
                NHibernateUtil.Guid.NullSafeSet(cmd, picture.Identifier, ++index, session);

                IExternalStorageProvider provider = GetExternalStorageProvider(session);
                // TODO: the blob container name should not be there
                provider.SavePictureAsync(picture, "profiles").GetAwaiter().GetResult();
            }
        }
    }
}