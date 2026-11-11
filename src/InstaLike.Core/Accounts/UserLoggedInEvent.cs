using InstaLike.Core.SharedKernel;

namespace InstaLike.Core.Accounts;

/// <summary>
/// Occurs when a user logs in.
/// </summary>
/// <param name="UserID">The identifier of the logged-in user.</param>
public sealed record UserLoggedInEvent(int UserID) : IDomainEvent;
