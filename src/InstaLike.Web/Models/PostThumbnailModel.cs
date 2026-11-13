using InstaLike.Core.Posts;
using InstaLike.Core.SharedKernel;

namespace InstaLike.Web.Models
{
    public sealed class PostThumbnailModel
    {
        public required PostId PostID { get; set; }
        public PictureId? Picture { get; set; }
    }
}
