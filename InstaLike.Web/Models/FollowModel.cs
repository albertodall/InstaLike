using System;

namespace InstaLike.Web.Models
{
    public class FollowModel
    {
        public string Nickname { get; set; } = string.Empty;
        public byte[] ProfilePicture { get; set; } = Array.Empty<byte>();
    }
}
