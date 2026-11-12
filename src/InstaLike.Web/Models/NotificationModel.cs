using System;
using InstaLike.Core.Notifications;
using InstaLike.Core.SharedKernel;

namespace InstaLike.Web.Models;

public sealed class NotificationModel
{
    public string SenderNickname { get; set; } = string.Empty;
    public PictureId? SenderProfilePicture { get; set; }
    public NotificationKind Kind { get; set; }
    public int? PostID { get; set; }
    public DateTimeOffset NotificationDate { get; set; }
}
