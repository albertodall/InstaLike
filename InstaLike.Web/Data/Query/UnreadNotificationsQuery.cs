using System;
using System.Threading;
using System.Threading.Tasks;
using InstaLike.Core.Domain;
using MediatR;
using NHibernate;

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
            int result = 0;
            using (var tx = _session.BeginTransaction())
            {
                var countQuery = _session.QueryOver<Notification>()
                    .Where(n => n.Recipient.ID == request.UserID) // Filter notifications to read
                    .ToRowCountQuery();

                result = await countQuery.RowCountAsync();
            }

            return result;
        }
    }
}
