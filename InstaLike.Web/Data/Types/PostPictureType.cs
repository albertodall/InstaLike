using InstaLike.Web.Services;

namespace InstaLike.Web.Data.Types
{
    internal class PostPictureType : ExternalStoragePictureType
    {
        public PostPictureType() : base(PictureContainers.Posts) { }
    }
}
