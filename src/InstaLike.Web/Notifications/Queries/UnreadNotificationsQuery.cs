using System;
using System.Threading;
using System.Threading.Tasks;
using InstaLike.Core.Notifications;
using MediatR;
using NHibernate;
using Serilog;

namespace InstaLike.Web.Notifications.Queries
{
    public sealed class UnreadNotificationsQuery : IRequest<int>
    {
        public Guid UserID { get; }

        public UnreadNotificationsQuery(Guid userID)
        {
            UserID = userID;
        }
    }

    internal sealed class UnreadNotificationsQueryHandler : IRequestHandler<UnreadNotificationsQuery, int>
    {
        private readonly IStatelessSession _session;
        private readonly ILogger _logger;

        public UnreadNotificationsQueryHandler(IStatelessSession session, ILogger logger)
        {
            _session = session;
            _logger = logger.ForContext<UnreadNotificationsQuery>();
        }

        public async Task<int> Handle(UnreadNotificationsQuery request, CancellationToken cancellationToken)
        {
            int unreadNotifications;

            _logger.Debug("Counting unread notifications for user {UserID}", request.UserID);

            using (var tx = _session.BeginTransaction())
            {
                var countQuery = _session.QueryOver<Notification>()
                    .Where(n => n.RecipientID == request.UserID).AndNot(n => n.HasBeenReadByRecipient);

                unreadNotifications = await countQuery.RowCountAsync(cancellationToken);

                await tx.CommitAsync(cancellationToken);
            }

            if (unreadNotifications > 0)
            {
                _logger.Information("User {UserID} has {UnreadNotifications} unread notifications", request.UserID, unreadNotifications);
            }

            return unreadNotifications;
        }
    }
}
