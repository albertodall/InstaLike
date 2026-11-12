using InstaLike.Core.SharedKernel;

namespace InstaLike.Web.Models;

public sealed class PostThumbnailModel
{
    public int PostID { get; set; }
    public PictureId? Picture { get; set; }
}
