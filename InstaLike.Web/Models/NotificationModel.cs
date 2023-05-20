using System;

namespace InstaLike.Web.Models
{
    public class NotificationModel
    {
        public string SenderNickname { get; set; } = string.Empty;
        public byte[] SenderProfilePicture { get; set; } = Array.Empty<byte>();
        public string Message { get; set; } = string.Empty;
        public DateTimeOffset NotificationDate { get; set; }
    }
}
