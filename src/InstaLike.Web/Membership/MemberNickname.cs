using System.Threading;
using System.Threading.Tasks;
using InstaLike.Core.Membership;
using InstaLike.Core.SharedKernel;
using MediatR;
using NHibernate;

namespace InstaLike.Web.Membership
{
    public sealed class MemberNicknameQuery : IRequest<string>
    {
        public MemberId MemberID { get; }

        public MemberNicknameQuery(MemberId memberID)
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
            using var tx = _session.BeginTransaction();

            var nickname = await _session.QueryOver<Member>()
                .Where(m => m.ID == request.MemberID)
                .Select(m => m.Nickname)
                .SingleOrDefaultAsync<string>(cancellationToken);

            await tx.CommitAsync(cancellationToken);
            return nickname;
        }
    }
}
