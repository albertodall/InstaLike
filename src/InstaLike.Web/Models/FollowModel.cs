using InstaLike.Core.SharedKernel;

namespace InstaLike.Web.Models;

public sealed class FollowModel
{
    public string Nickname { get; set; } = string.Empty;
    public PictureId? ProfilePicture { get; set; }
}
