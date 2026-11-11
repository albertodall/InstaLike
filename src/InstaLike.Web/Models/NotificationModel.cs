using System;
using InstaLike.Core.SharedKernel;

namespace InstaLike.Web.Models;

public class NotificationModel
{
    public string SenderNickname { get; set; } = string.Empty;
    public PictureId? SenderProfilePicture { get; set; }
    public string Message { get; set; } = string.Empty;
    public DateTimeOffset NotificationDate { get; set; }
}
