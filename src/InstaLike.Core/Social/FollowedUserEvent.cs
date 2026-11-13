using System;
using InstaLike.Core.SharedKernel;

namespace InstaLike.Core.Social;

public sealed record FollowedUserEvent(Guid FollowerID, Guid FollowedID) : IDomainEvent;
