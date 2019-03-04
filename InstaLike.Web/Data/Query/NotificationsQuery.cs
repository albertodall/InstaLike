using System;
using System.Threading;
using System.Threading.Tasks;
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
            throw new NotImplementedException();
        }
    }
}
