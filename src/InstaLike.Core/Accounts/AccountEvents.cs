using InstaLike.Core.SharedKernel;

namespace InstaLike.Core.Accounts
{
    /// <summary>
    /// Occurs when an account logs in.
    /// </summary>
    /// <param name="AccountID">The identifier of the account that logged in.</param>
    public sealed record AccountLoggedInEvent(AccountId AccountID) : IDomainEvent;

    /// <summary>
    /// Occurs when an account logs out.
    /// </summary>
    /// <param name="AccountID">The identifier of the account that logged out.</param>
    public sealed record AccountLoggedOutEvent(AccountId AccountID) : IDomainEvent;

    /// <summary>
    /// Occurs when an account changes its password.
    /// </summary>
    /// <param name="AccountID">The identifier of the account that changed its password.</param>
    public sealed record AccountPasswordChangedEvent(AccountId AccountID) : IDomainEvent;
}
