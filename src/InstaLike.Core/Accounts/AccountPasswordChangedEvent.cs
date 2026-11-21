using InstaLike.Core.SharedKernel;

namespace InstaLike.Core.Accounts
{
    /// <summary>
    /// Occurs when an account changes its password.
    /// </summary>
    /// <param name="AccountID">The identifier of the account that changed its password.</param>
    public sealed record AccountPasswordChangedEvent(AccountId AccountID) : IDomainEvent;
}
