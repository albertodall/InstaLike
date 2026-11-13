using InstaLike.Core.SharedKernel;

namespace InstaLike.Core.Membership;

/// <summary>
/// Represents an event raised when a member logs out.
/// </summary>
/// <param name="MemberID">The ID of the member who logged out.</param>
public sealed record MemberLoggedOutEvent(MemberId MemberID) : IDomainEvent;
