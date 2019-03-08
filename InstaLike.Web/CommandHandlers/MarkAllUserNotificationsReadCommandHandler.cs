using System;
using System.Threading;
using System.Threading.Tasks;
using InstaLike.Core.Commands;
using InstaLike.Core.Domain;
using MediatR;
using NHibernate;
using NHibernate.Criterion;

namespace InstaLike.Web.CommandHandlers
{
    public class MarkAllUserNotificationsReadCommandHandler : IRequestHandler<MarkAllUserNotificationsReadCommand, int>
    {
        private readonly ISession _session;

        public MarkAllUserNotificationsReadCommandHandler(ISession session)
        {
            _session = session ?? throw new ArgumentNullException(nameof(session));
        }

        public async Task<int> Handle(MarkAllUserNotificationsReadCommand request, CancellationToken cancellationToken)
        {
            using (var tx = _session.BeginTransaction())
            {
                try
                {
                    var notificationsQuery = QueryOver.Of<Notification>()
                        .Where(n => n.Recipient.ID == request.UserID).AndNot(n => n.HasBeenReadByRecipient);

                    var unreadNotificationsCountQuery = notificationsQuery.GetExecutableQueryOver(_session).ToRowCountQuery().FutureValue<int>();
                    var unreadNotificationsQuery = notificationsQuery.GetExecutableQueryOver(_session).Future<Notification>();

                    foreach (var n in (await unreadNotificationsQuery.GetEnumerableAsync()))
                    {
                        n.MarkAsRead();
                    }

                    await tx.CommitAsync();
                    return await unreadNotificationsCountQuery.GetValueAsync();
                }
                catch (ADOException)
                {
                    await tx.RollbackAsync();
                    throw;
                }
            }
        }
    }
}
