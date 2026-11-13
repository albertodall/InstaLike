using System.Threading;
using System.Threading.Tasks;
using InstaLike.Core.Accounts;
using InstaLike.Core.Membership;
using MediatR;
using NHibernate;
using Serilog;

namespace InstaLike.Web.Accounts.EventHandlers;

internal sealed class MemberLoggedInEventHandler : INotificationHandler<MemberLoggedInEvent>
{
    private readonly ISession _session;
    private readonly ILogger _logger;

    public MemberLoggedInEventHandler(ISession session, ILogger logger)
    {
        _session = session;
        _logger = logger.ForContext<MemberLoggedInEvent>();
    }

    public async Task Handle(MemberLoggedInEvent notification, CancellationToken cancellationToken)
    {
        var member = await _session.GetAsync<Member>(notification.MemberID, cancellationToken);
        _logger.Information("Member [{Nickname}({MemberID})] has just logged in.", member?.Nickname, notification.MemberID);
    }
}
