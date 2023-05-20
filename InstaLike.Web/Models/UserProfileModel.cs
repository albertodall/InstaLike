using System;

namespace InstaLike.Web.Models
{
    public record UserProfileModel
    {
        public int UserID { get; set; }
        public string Nickname { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Surname { get; set; } = string.Empty;
        public string Bio { get; set; } = string.Empty;
        public int NumberOfPosts { get; set; }
        public int NumberOfFollowers { get; set; }
        public int NumberOfFollows { get; set; }
        public bool IsCurrentUserProfile { get; set; }
        public bool Following { get; set; }
        public byte[] ProfilePictureBytes { get; set; } = Array.Empty<byte>();
        public PostThumbnailModel[] RecentPosts { get; set; } = Array.Empty<PostThumbnailModel>();
    }
}
