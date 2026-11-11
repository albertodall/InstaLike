using InstaLike.Web.Data;

namespace InstaLike.Web.Notifications.Commands;

public sealed class MarkAllUserNotificationsReadCommand : ICommand<int>
{
    public int UserID { get; }

    public MarkAllUserNotificationsReadCommand(int userID)
    {
        UserID = userID;
    }
}
