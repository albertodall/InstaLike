#nullable disable

namespace InstaLike.Web.Models
{
    public class PostThumbnailModel
    {
        public int PostID { get; set; }
        public byte[] ThumbnailPictureBytes { get; set; }
    }
}