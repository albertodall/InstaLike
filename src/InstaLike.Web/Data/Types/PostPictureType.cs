using InstaLike.Web.Media;

namespace InstaLike.Web.Data.Types;

internal class PostPictureType : ExternalStoragePictureType
{
    public PostPictureType() : base(PictureContainers.Posts) { }
}
