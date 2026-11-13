using System;
using InstaLike.Core.SharedKernel;

namespace InstaLike.Core.Social
{
    public sealed record MemberFollowedEvent(Guid FollowerID, Guid FollowedID) : IDomainEvent;
}
