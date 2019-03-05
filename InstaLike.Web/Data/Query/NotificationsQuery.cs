using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using InstaLike.Core.Domain;
using InstaLike.Web.Models;
using MediatR;
using NHibernate;

namespace InstaLike.Web.Data.Query
{
    public class NotificationsQuery : IRequest<NotificationModel[]>
    {
        public int UserID { get; }
        public bool IncludeReadNotifications { get; }

        public NotificationsQuery(int userID, bool includeReadNotifications)
        {
            UserID = UserID;
            IncludeReadNotifications = includeReadNotifications;
        }
    }

    internal class NotificationsQueryHandler : IRequestHandler<NotificationsQuery, NotificationModel[]>
    {
        private readonly ISession _session;

        public NotificationsQueryHandler(ISession session)
        {
            _session = session ?? throw new ArgumentNullException(nameof(session));
        }

        public async Task<NotificationModel[]> Handle(NotificationsQuery request, CancellationToken cancellationToken)
        {
            NotificationModel notification = null;

            using (var tx = _session.BeginTransaction())
            {
                var notificationsQuery = _session.QueryOver<Notification>()
                    .Fetch(SelectMode.FetchLazyProperties, n => n.Sender)
                    .Where(n => n.Recipient.ID == request.UserID);

                if (!request.IncludeReadNotifications)
                {
                    notificationsQuery.And(n => !n.IsRead);
                }

                notificationsQuery
                    .OrderBy(n => n.NotificationDate).Desc
                    .SelectList(fields => fields
                        .Select(n => n.Sender.Nickname.Value).WithAlias(() => notification.SenderNickname)
                        .Select(n => n.Sender.ProfilePicture.RawBytes).WithAlias(() => notification.SenderProfilePicture)
                        .Select(n => n.Message).WithAlias(() => notification.Message)
                        .Select(n => n.NotificationDate).WithAlias(() => notification.NotificationDate)
                    );

                return (await notificationsQuery.ListAsync<NotificationModel>()).ToArray();
            }
        }
    }
}
