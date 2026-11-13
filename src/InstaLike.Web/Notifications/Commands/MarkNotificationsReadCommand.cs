using System;
using System.Collections.Generic;
using InstaLike.Web.Data;

namespace InstaLike.Web.Notifications.Commands;

public sealed class MarkNotificationsReadCommand : ICommand<int>
{
    public Guid RecipientID { get; }
    public IReadOnlyCollection<Guid> NotificationIDs { get; }

    public MarkNotificationsReadCommand(Guid recipientID, IReadOnlyCollection<Guid> notificationIDs)
    {
        RecipientID = recipientID;
        NotificationIDs = notificationIDs;
    }
}
