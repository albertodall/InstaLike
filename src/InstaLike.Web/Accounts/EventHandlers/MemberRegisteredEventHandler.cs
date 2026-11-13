using System.Threading;
using System.Threading.Tasks;
using InstaLike.Core.Accounts;
using InstaLike.Core.Membership;
using MediatR;
using NHibernate;

namespace InstaLike.Web.Accounts.EventHandlers;

/// <summary>
/// Opens the account a new member logs in with, in the same transaction that registers the member.
/// </summary>
internal sealed class MemberRegisteredEventHandler : INotificationHandler<MemberRegisteredEvent>
{
    private readonly ISession _session;

    public MemberRegisteredEventHandler(ISession session)
    {
        _session = session;
    }

    public Task Handle(MemberRegisteredEvent notification, CancellationToken cancellationToken) =>
        _session.SaveAsync(
            Account.Create(notification.MemberID.Value, notification.Username, notification.Password, notification.Email),
            cancellationToken);
}
