using System.Threading;
using System.Threading.Tasks;
using InstaLike.Core.Accounts;
using InstaLike.Web.Data;
using InstaLike.Web.Models;
using MediatR;
using NHibernate;
using Serilog;

namespace InstaLike.Web.Accounts.Queries;

internal sealed class UserDetailsQuery : IRequest<UserDetailsModel>
{
    public UserId CurrentUserId { get; }

    public UserDetailsQuery(UserId currentUserId)
    {
        CurrentUserId = currentUserId;
    }
}

internal sealed class UserDetailsQueryHandler : IRequestHandler<UserDetailsQuery, UserDetailsModel>
{
    private readonly IStatelessSession _session;
    private readonly ILogger _logger;

    public UserDetailsQueryHandler(IStatelessSession session, ILogger logger)
    {
        _session = session;
        _logger = logger.ForContext<UserDetailsQuery>();
    }

    public async Task<UserDetailsModel> Handle(UserDetailsQuery request, CancellationToken cancellationToken)
    {
        UserDetailsModel result;

        _logger.Information("Reading user details of user {UserID} with parameters {@Request}", request.CurrentUserId, request);

        using (var tx = _session.BeginTransaction())
        {
            UserDetailsModel userDetails = null!;
            result = await _session.QueryOver<User>()
                .Where(u => u.ID == request.CurrentUserId)
                .SelectList(list => list
                    .Select(u => u.Nickname).WithAlias(() => userDetails.Nickname)
                    .Select(u => u.FullName.Name).WithAlias(() => userDetails.Name)
                    .Select(u => u.FullName.Surname).WithAlias(() => userDetails.Surname)
                    .Select(u => u.Email).WithAlias(() => userDetails.Email)
                    .Select(u => u.Biography).WithAlias(() => userDetails.Bio)
                    .Select(u => u.ProfilePicture).WithAlias(() => userDetails.ProfilePicture)
                )
                .TransformUsing(new EntityToModelResultTransformer<UserDetailsModel>())
                .SingleOrDefaultAsync<UserDetailsModel>(cancellationToken);

            await tx.CommitAsync(cancellationToken);
        }

        return result;
    }
}
