using InstaLike.Core.SharedKernel;

namespace InstaLike.Core.Social
{
    public sealed record MemberFollowedEvent(MemberId FollowerID, MemberId FollowedID) : IDomainEvent;
}
