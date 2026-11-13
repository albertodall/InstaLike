using System;
using InstaLike.Web.Data;

namespace InstaLike.Web.Notifications.Commands;

public sealed class MarkAllUserNotificationsReadCommand : ICommand<int>
{
    public Guid UserID { get; }

    public MarkAllUserNotificationsReadCommand(Guid userID)
    {
        UserID = userID;
    }
}
