using System.Threading;
using System.Threading.Tasks;
using InstaLike.Core.Membership;
using InstaLike.Web.Data;
using InstaLike.Web.Models;
using MediatR;
using NHibernate;
using Serilog;

namespace InstaLike.Web.Membership.Queries
{
    internal sealed class MemberDetailsQuery : IRequest<MemberDetailsModel>
    {
        public MemberId MemberID { get; }

        public MemberDetailsQuery(MemberId memberID)
        {
            MemberID = memberID;
        }
    }

    internal sealed class MemberDetailsQueryHandler : IRequestHandler<MemberDetailsQuery, MemberDetailsModel>
    {
        private readonly IStatelessSession _session;
        private readonly ILogger _logger;

        public MemberDetailsQueryHandler(IStatelessSession session, ILogger logger)
        {
            _session = session;
            _logger = logger.ForContext<MemberDetailsQuery>();
        }

        public async Task<MemberDetailsModel> Handle(MemberDetailsQuery request, CancellationToken cancellationToken)
        {
            MemberDetailsModel result;

            _logger.Information("Reading member details of member {MemberID} with parameters {@Request}", request.MemberID, request);

            using (var tx = _session.BeginTransaction())
            {
                MemberDetailsModel memberDetails = null!;
                result = await _session.QueryOver<Member>()
                    .Where(m => m.ID == request.MemberID)
                    .SelectList(list => list
                        .Select(m => m.Nickname).WithAlias(() => memberDetails.Nickname)
                        .Select(m => m.FullName.Name).WithAlias(() => memberDetails.Name)
                        .Select(m => m.FullName.Surname).WithAlias(() => memberDetails.Surname)
                        .Select(m => m.Biography).WithAlias(() => memberDetails.Bio)
                        .Select(m => m.ProfilePicture).WithAlias(() => memberDetails.ProfilePicture)
                    )
                    .TransformUsing(new EntityToModelResultTransformer<MemberDetailsModel>())
                    .SingleOrDefaultAsync<MemberDetailsModel>(cancellationToken);

                await tx.CommitAsync(cancellationToken);
            }

            return result;
        }
    }
}
