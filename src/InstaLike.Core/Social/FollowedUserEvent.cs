using InstaLike.Core.SharedKernel;

namespace InstaLike.Core.Social;

public sealed record FollowedUserEvent(int FollowerID, int FollowedID) : IDomainEvent;
