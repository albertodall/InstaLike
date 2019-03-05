using InstaLike.Core.Domain;
using MediatR;
using NHibernate;
using System;
using System.Threading;
using System.Threading.Tasks;

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

        public UnreadNotificationsQueryHandler(ISession session)
        {
            _session = session ?? throw new ArgumentNullException(nameof(session));
        }

        public async Task<int> Handle(UnreadNotificationsQuery request, CancellationToken cancellationToken)
        {
            int count = 5;
            //using (var tx = _session.BeginTransaction())
            //{
            //    count = await _session.QueryOver<Notification>()
            //        .Where(n => n.Recipient.ID == request.UserID).And(n => !n.IsRead)
            //        .RowCountAsync();
            //}

            return await Task.FromResult(count);
        }
    }
}
