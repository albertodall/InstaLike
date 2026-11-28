using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using InstaLike.Core.Accounts;
using InstaLike.Core.Membership;
using InstaLike.Core.Notifications;
using InstaLike.Core.SharedKernel;
using InstaLike.Web.Data;
using MediatR;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.SqlCommand;
using Serilog;

namespace InstaLike.Web.Notifications
{
    public sealed class NotificationModel
    {
        public NotificationId ID { get; set; } = null!;
        public string SenderNickname { get; set; } = string.Empty;
        public PictureId? SenderProfilePicture { get; set; }
        public NotificationKind Kind { get; set; }
        public PostId? PostID { get; set; }
        public DateTimeOffset NotificationDate { get; set; }
    }

    public sealed class NotificationsQuery : IRequest<NotificationModel[]>
    {
        public MemberId UserID { get; }
        public bool IncludeReadNotifications { get; }

        public NotificationsQuery(MemberId userID, bool includeReadNotifications)
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
                Notification notification = null!;
                Member sender = null!;

                var notificationsQuery = _session.QueryOver(() => notification)
                    .JoinEntityAlias(() => sender,
                        Restrictions.EqProperty(
                            Projections.Property(() => sender.ID),
                            Projections.Property(() => notification.SenderID)),
                        JoinType.InnerJoin)
                    .Where(() => notification.RecipientID == request.UserID);

                if (!request.IncludeReadNotifications)
                {
                    notificationsQuery.And(() => !notification.HasBeenReadByRecipient);
                }

                notificationsQuery
                    .OrderBy(() => notification.NotificationDate).Desc
                    .SelectInto(() => new NotificationModel
                    {
                        ID = notification.ID,
                        SenderNickname = sender.Nickname,
                        SenderProfilePicture = sender.ProfilePicture,
                        Kind = notification.Kind,
                        PostID = notification.PostID,
                        NotificationDate = notification.NotificationDate
                    });

                notifications = (await notificationsQuery.ListAsync<NotificationModel>(cancellationToken)).ToArray();
            }

            return notifications;
        }
    }
}
