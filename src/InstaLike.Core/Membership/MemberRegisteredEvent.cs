using InstaLike.Core.Accounts;
using InstaLike.Core.SharedKernel;

namespace InstaLike.Core.Membership
{
    /// <summary>
    /// Occurs when a member registers, carrying the credentials the member's account is created with.
    /// </summary>
    /// <param name="MemberID">The identifier of the registered member.</param>
    /// <param name="Username">The name the member logs in with.</param>
    /// <param name="Password">The member's password, already hashed.</param>
    /// <param name="Email">The member's email address.</param>
    public sealed record MemberRegisteredEvent(MemberId MemberID, Username Username, Password Password, Email Email) : IDomainEvent;
}
