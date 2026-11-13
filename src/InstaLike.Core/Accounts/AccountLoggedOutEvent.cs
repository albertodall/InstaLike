using InstaLike.Core.SharedKernel;

namespace InstaLike.Core.Accounts
{
    /// <summary>
    /// Occurs when an account logs out.
    /// </summary>
    /// <param name="AccountID">The identifier of the account that logged out.</param>
    public sealed record AccountLoggedOutEvent(AccountId AccountID) : IDomainEvent;
}
