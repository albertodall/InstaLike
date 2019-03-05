using System;

namespace InstaLike.Web.Models
{
    public class NotificationModel
    {
        public string SenderNickname { get; set; }
        public byte[] SenderProfilePicture { get; set; }
        public string Message { get; set; }
        public DateTimeOffset NotificationDate { get; set; }
    }
}
