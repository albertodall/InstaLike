using System.Threading;
using System.Threading.Tasks;
using InstaLike.Core.Accounts;
using InstaLike.Core.Membership;
using MediatR;
using NHibernate;
using Serilog;

namespace InstaLike.Web.Accounts.EventHandlers;

internal sealed class MemberLoggedOutEventHandler : INotificationHandler<MemberLoggedOutEvent>
{
    private readonly ISession _session;
    private readonly ILogger _logger;

    public MemberLoggedOutEventHandler(ISession session, ILogger logger)
    {
        _session = session;
        _logger = logger.ForContext<MemberLoggedOutEvent>();
    }

    public async Task Handle(MemberLoggedOutEvent notification, CancellationToken cancellationToken)
    {
        var member = await _session.GetAsync<Member>(notification.MemberID, cancellationToken);
        _logger.Information("Member [{Nickname}({MemberID})] has just logged out.", member?.Nickname, notification.MemberID);
    }
}
