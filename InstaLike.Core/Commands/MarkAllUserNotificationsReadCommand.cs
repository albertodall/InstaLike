using MediatR;

namespace InstaLike.Core.Commands
{
    public class MarkAllUserNotificationsReadCommand : IRequest<int>
    {
        public int UserID { get; }

        public MarkAllUserNotificationsReadCommand(int userID)
        {
            UserID = userID;
        }

    }
}