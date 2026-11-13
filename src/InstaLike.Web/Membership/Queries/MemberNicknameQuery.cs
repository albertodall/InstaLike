using System;
using System.Threading;
using System.Threading.Tasks;
using InstaLike.Core.Membership;
using MediatR;
using NHibernate;

namespace InstaLike.Web.Membership.Queries
{
    public sealed class MemberNicknameQuery : IRequest<string>
    {
        public Guid MemberID { get; }

        public MemberNicknameQuery(Guid memberID)
        {
            MemberID = memberID;
        }
    }

    internal sealed class MemberNicknameQueryHandler : IRequestHandler<MemberNicknameQuery, string>
    {
        private readonly IStatelessSession _session;

        public MemberNicknameQueryHandler(IStatelessSession session)
        {
            _session = session;
        }

        public async Task<string> Handle(MemberNicknameQuery request, CancellationToken cancellationToken)
        {
            var memberID = MemberId.From(request.MemberID);
            using var tx = _session.BeginTransaction();

            var nickname = await _session.QueryOver<Member>()
                .Where(m => m.ID == memberID)
                .Select(m => m.Nickname)
                .SingleOrDefaultAsync<string>(cancellationToken);

            await tx.CommitAsync(cancellationToken);
            return nickname;
        }
    }
}
