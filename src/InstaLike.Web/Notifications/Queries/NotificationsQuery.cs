using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using InstaLike.Core.Accounts;
using InstaLike.Core.Membership;
using InstaLike.Core.Notifications;
using InstaLike.Web.Data;
using InstaLike.Web.Models;
using MediatR;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.SqlCommand;
using Serilog;

namespace InstaLike.Web.Notifications.Queries;

public sealed class NotificationsQuery : IRequest<NotificationModel[]>
{
    public Guid UserID { get; }
    public bool IncludeReadNotifications { get; }

    public NotificationsQuery(Guid userID, bool includeReadNotifications)
    {
        UserID = userID;
        IncludeReadNotifications = includeReadNotifications;
    }
}

internal sealed class NotificationsQueryHandler : IRequestHandler<NotificationsQuery, NotificationModel[]>
{
    private readonly IStatelessSession _session;
    private readonly ILogger _logger;

    public NotificationsQueryHandler(IStatelessSession session, ILogger logger)
    {
        _session = session;
        _logger = logger.ForContext<NotificationsQuery>();
    }

    public async Task<NotificationModel[]> Handle(NotificationsQuery request, CancellationToken cancellationToken)
    {
        NotificationModel[] notifications;

        _logger.Debug("Reading notifications for user {UserID} with parameters: {@Request}", request.UserID, request);

        using (var tx = _session.BeginTransaction())
        {
            NotificationModel notification = null!;
            Notification notificationEntity = null!;
            Member sender = null!;

            var notificationsQuery = _session.QueryOver(() => notificationEntity)
                .JoinEntityAlias(() => sender, Restrictions.EqProperty(Projections.Property(() => sender.ID), Projections.Property(() => notificationEntity.SenderID)), JoinType.InnerJoin)
                .Where(() => notificationEntity.RecipientID == request.UserID);

            if (!request.IncludeReadNotifications)
            {
                notificationsQuery.And(() => !notificationEntity.HasBeenReadByRecipient);
            }

            notificationsQuery
                .OrderBy(() => notificationEntity.NotificationDate).Desc
                .SelectList(fields => fields
                    .Select(() => notificationEntity.ID).WithAlias(() => notification.ID)
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
