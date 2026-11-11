using InstaLike.Core.SharedKernel;

namespace InstaLike.Core.Notifications;

public sealed class MarkAllUserNotificationsReadCommand : ICommand<int>
{
    public int UserID { get; }

    public MarkAllUserNotificationsReadCommand(int userID)
    {
        UserID = userID;
    }

}
