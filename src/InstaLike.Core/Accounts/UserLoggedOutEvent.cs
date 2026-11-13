using InstaLike.Core.SharedKernel;

namespace InstaLike.Core.Accounts;

/// <summary>
/// Represents an event raised when a user logs out.
/// </summary>
/// <param name="UserID">The ID of the user who logged out.</param>
public sealed record UserLoggedOutEvent(UserId UserID) : IDomainEvent;
