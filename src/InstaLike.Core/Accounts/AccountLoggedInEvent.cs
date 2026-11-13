using InstaLike.Core.SharedKernel;

namespace InstaLike.Core.Accounts;

/// <summary>
/// Occurs when an account logs in.
/// </summary>
/// <param name="AccountID">The identifier of the account that logged in.</param>
public sealed record AccountLoggedInEvent(AccountId AccountID) : IDomainEvent;
