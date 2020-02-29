using System;
using System.Data.Common;
using InstaLike.Core.Domain;
using NHibernate.Engine;

namespace InstaLike.Web.Data.Types
{
    internal class PostPictureType : ExternalStoragePictureType
    {
        private const string PostPicturesContainerName = "posts";

        public override object NullSafeGet(DbDataReader dr, string[] names, ISessionImplementor session, object owner)
        {
            var guidFieldName = names[1];
            var pictureBytesFieldName = names[0];

            return LoadPictureFromConfiguredProvider(dr, guidFieldName, pictureBytesFieldName, session, PostPicturesContainerName);
        }

        public override void NullSafeSet(DbCommand cmd, object value, int index, bool[] settable, ISessionImplementor session)
        {
            var picture = value as Picture;
            if (picture == null)
            {
                throw new ArgumentException(nameof(value), $"Specified value is not a {nameof(Picture)}");
            }

            SavePictureToConfiguredProvider(cmd, picture, index, session, PostPicturesContainerName);
        }
    }
}
