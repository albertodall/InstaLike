using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using InstaLike.Core.Domain;
using InstaLike.Web.Models;
using MediatR;
using NHibernate;
using NHibernate.Transform;

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

        public NotificationsQueryHandler(ISession session)
        {
            _session = session ?? throw new ArgumentNullException(nameof(session));
        }

        public async Task<NotificationModel[]> Handle(NotificationsQuery request, CancellationToken cancellationToken)
        {
            NotificationModel[] result = { };

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

                result = (await notificationsQuery.ListAsync<NotificationModel>()).ToArray();
            }

            return result;
        }
    }
}
