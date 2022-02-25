using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using InstaLike.Core.Domain;
using InstaLike.Web.Models;
using MediatR;
using NHibernate;
using NHibernate.Transform;
using Serilog;

#nullable disable

namespace InstaLike.Web.Data.Query
{
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
                NotificationModel notification = null;
                User recipient = null;

                var notificationsQuery = _session.QueryOver<Notification>()
                    .Inner.JoinAlias(n => n.Recipient, () => recipient)
                    .Where(n => n.Recipient.ID == request.UserID);

                if (!request.IncludeReadNotifications)
                {
                    notificationsQuery.And(n => !n.HasBeenReadByRecipient);
                }

                notificationsQuery
                    .OrderBy(n => n.NotificationDate).Desc
                    .SelectList(fields => fields
                        .Select(() => recipient.Nickname).WithAlias(() => notification.SenderNickname)
                        .Select(() => recipient.ProfilePicture.RawBytes).WithAlias(() => notification.SenderProfilePicture)
                        .Select(n => n.Message).WithAlias(() => notification.Message)
                        .Select(n => n.NotificationDate).WithAlias(() => notification.NotificationDate)
                    )
                    .TransformUsing(Transformers.AliasToBean<NotificationModel>());

                notifications = (await notificationsQuery.ListAsync<NotificationModel>(cancellationToken)).ToArray();
            }

            return notifications;
        }
    }
}
