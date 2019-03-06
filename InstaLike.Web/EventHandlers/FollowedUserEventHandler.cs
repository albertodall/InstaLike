using System;
using System.Threading;
using System.Threading.Tasks;
using InstaLike.Core.Domain;
using InstaLike.Core.Events;
using MediatR;
using NHibernate;
using NHibernate.Criterion;

namespace InstaLike.Web.EventHandlers
{
    public class FollowedUserEventHandler : INotificationHandler<FollowedUserEvent>
    {
        private const string NotificationMessageTemplate = "<a href=\"{0}\">{1}</a> has started following you.";

        private readonly ISession _session;

        public FollowedUserEventHandler(ISession session)
        {
            _session = session ?? throw new ArgumentNullException(nameof(session));
        }

        public async Task Handle(FollowedUserEvent notification, CancellationToken cancellationToken)
        {
            using (var tx = _session.BeginTransaction())
            {
                try
                {
                    var senderQuery = _session.QueryOver<User>()
                        .Where(Restrictions.Eq("Nickname", notification.SenderNickname))
                        .FutureValue();

                    var recipientQuery = _session.QueryOver<User>()
                       .Where(Restrictions.Eq("Nickname", notification.FollowingNickname))
                       .FutureValue();

                    var message = string.Format(NotificationMessageTemplate,
                        notification.SenderProfileUrl,
                        notification.SenderNickname);

                    var notificationToInsert = new Notification(await senderQuery.GetValueAsync(), await recipientQuery.GetValueAsync(), message);
                    await _session.SaveAsync(notificationToInsert);
                    await tx.CommitAsync();
                }
                catch (ADOException ex)
                {
                    await tx.RollbackAsync();
                    throw ex;
                }
            }
        }
    }
}
