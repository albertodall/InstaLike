using System;
using System.Threading;
using System.Threading.Tasks;
using InstaLike.Core.Domain;
using MediatR;
using NHibernate;
using Serilog;

namespace InstaLike.Web.Data.Query
{
    public class UnreadNotificationsQuery : IRequest<int>
    {
        public int UserID { get; }

        public UnreadNotificationsQuery(int userID)
        {
            UserID = userID;
        }
    }

    internal sealed class UnreadNotificationsQueryHandler : IRequestHandler<UnreadNotificationsQuery, int>
    {
        private readonly ISession _session;
        private readonly ILogger _logger;

        public UnreadNotificationsQueryHandler(ISession session, ILogger logger)
        {
            _session = session ?? throw new ArgumentNullException(nameof(session));
            _logger = logger?.ForContext<UnreadNotificationsQuery>() ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<int> Handle(UnreadNotificationsQuery request, CancellationToken cancellationToken)
        {
            int unreadNotifications = 0;

            _logger.Debug("Counting unread notifications for user {UserID}", request.UserID);

            using (var tx = _session.BeginTransaction())
            {
                var countQuery = _session.QueryOver<Notification>()
                    .Where(n => n.Recipient.ID == request.UserID).AndNot(n => n.HasBeenReadByRecipient);

                unreadNotifications = await countQuery.RowCountAsync();
            }

            if (unreadNotifications > 0)
            {
                _logger.Information("User {UserID} has {UnreadNotifications} unread notifications", request.UserID, unreadNotifications);
            }

            return unreadNotifications;
        }
    }
}
