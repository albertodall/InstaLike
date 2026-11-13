using System;
using InstaLike.Core.Accounts;
using InstaLike.Core.SharedKernel;

namespace InstaLike.Web.Models;

public record UserProfileModel
{
    public required UserId UserID { get; set; }
    public string Nickname { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Surname { get; set; } = string.Empty;
    public string Bio { get; set; } = string.Empty;
    public int NumberOfPosts { get; set; }
    public int NumberOfFollowers { get; set; }
    public int NumberOfFollows { get; set; }
    public bool IsCurrentUserProfile { get; set; }
    public bool Following { get; set; }
    public PictureId? ProfilePicture { get; set; }
    public PostThumbnailModel[] RecentPosts { get; set; } = Array.Empty<PostThumbnailModel>();
}
