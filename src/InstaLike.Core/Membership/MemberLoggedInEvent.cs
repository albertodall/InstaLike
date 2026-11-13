using InstaLike.Core.SharedKernel;

namespace InstaLike.Core.Membership;

/// <summary>
/// Occurs when a member logs in.
/// </summary>
/// <param name="MemberID">The identifier of the logged-in member.</param>
public sealed record MemberLoggedInEvent(MemberId MemberID) : IDomainEvent;
