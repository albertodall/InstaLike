using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using InstaLike.Core.Accounts;
using InstaLike.Core.Notifications;
using InstaLike.Web.Data;
using InstaLike.Web.Models;
using MediatR;
using NHibernate;
using Serilog;

namespace InstaLike.Web.Notifications.Queries;

public class NotificationsQuery : IRequest<NotificationModel[]>
{
    public int UserID { get; }
    public bool IncludeReadNotifications { get; }

    public NotificationsQuery(int userID, bool includeReadNotifications)
    {
        UserID = userID;
        IncludeReadNotifications = includeReadNotifications;
    }
}

internal class NotificationsQueryHandler : IRequestHandler<NotificationsQuery, NotificationModel[]>
{
    private readonly ISession _session;
    private readonly ILogger _logger;

    public NotificationsQueryHandler(ISession session, ILogger logger)
    {
        _session = session ?? throw new ArgumentNullException(nameof(session));
        _logger = logger?.ForContext<NotificationsQuery>() ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<NotificationModel[]> Handle(NotificationsQuery request, CancellationToken cancellationToken)
    {
        NotificationModel[] notifications;

        _logger.Debug("Reading notifications for user {UserID} with parameters: {@Request}", request.UserID, request);

        using (var tx = _session.BeginTransaction())
        {
            NotificationModel notification = null!;
            Notification notificationEntity = null!;
            User sender = null!;

            var notificationsQuery = _session.QueryOver(() => notificationEntity)
                .JoinEntityAlias(() => sender, () => sender.ID == notificationEntity.SenderID)
                .Where(() => notificationEntity.RecipientID == request.UserID);

            if (!request.IncludeReadNotifications)
            {
                notificationsQuery.And(() => !notificationEntity.HasBeenReadByRecipient);
            }

            notificationsQuery
                .OrderBy(() => notificationEntity.NotificationDate).Desc
                .SelectList(fields => fields
                    .Select(() => sender.Nickname).WithAlias(() => notification.SenderNickname)
                    .Select(() => sender.ProfilePicture).WithAlias(() => notification.SenderProfilePicture)
                    .Select(() => notificationEntity.Kind).WithAlias(() => notification.Kind)
                    .Select(() => notificationEntity.PostID).WithAlias(() => notification.PostID)
                    .Select(() => notificationEntity.NotificationDate).WithAlias(() => notification.NotificationDate)
                )
                .TransformUsing(new EntityToModelResultTransformer<NotificationModel>());

            notifications = (await notificationsQuery.ListAsync<NotificationModel>(cancellationToken)).ToArray();
        }

        return notifications;
    }
}
