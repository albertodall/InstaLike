using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using InstaLike.Core.Notifications;
using InstaLike.Core.Posts;
using MediatR;
using NHibernate;
using NHibernate.Linq;

namespace InstaLike.Web.Notifications
{
    internal sealed class PostDeletedEventHandler : INotificationHandler<PostDeletedEvent>
    {
        private readonly ISession _session;

        public PostDeletedEventHandler(ISession session)
        {
            _session = session;
        }

        public async Task Handle(PostDeletedEvent notification, CancellationToken cancellationToken)
        {
            await _session.Query<Notification>()
                .Where(n => n.PostID == notification.PostID.Value)
                .DeleteAsync(cancellationToken);
        }
    }
}
