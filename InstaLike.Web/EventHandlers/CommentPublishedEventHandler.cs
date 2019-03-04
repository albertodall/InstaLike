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
    public class CommentPublishedEventHandler : INotificationHandler<CommentPublishedEvent>
    {
        private const string NotificationMessageTemplate = "<a href=\"{0}\">{1}</a> wrote a comment <a href=\"{2}\">about your post.</a>";

        private readonly ISession _session;

        public CommentPublishedEventHandler(ISession session)
        {
            _session = session ?? throw new ArgumentNullException(nameof(session));
        }

        public async Task Handle(CommentPublishedEvent notification, CancellationToken cancellationToken)
        {
            using (var tx = _session.BeginTransaction())
            {
                try
                {
                    var postQuery = _session.QueryOver<Post>()
                        .Fetch(SelectMode.Fetch, p => p.Author)
                        .Where(p => p.ID == notification.PostID)
                        .FutureValue();

                    var senderQuery = _session.QueryOver<User>()
                        .Where(Restrictions.Eq("Nickname", notification.SenderNickname))
                        .FutureValue();

                    var post = await postQuery.GetValueAsync();
                    var message = string.Format(NotificationMessageTemplate, 
                        notification.SenderProfileUrl, 
                        post.Author.Nickname,
                        notification.PostUrl);

                    var notificationToInsert = new Notification(await senderQuery.GetValueAsync(), post.Author, message);

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