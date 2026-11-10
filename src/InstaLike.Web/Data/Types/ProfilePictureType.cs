using InstaLike.Web.Services;

namespace InstaLike.Web.Data.Types;

internal class ProfilePictureType : ExternalStoragePictureType
{
    public ProfilePictureType() : base(PictureContainers.Profiles) { }
}
