using System;
using System.Security.Claims;
using InstaLike.Core.Accounts;
using InstaLike.Core.Membership;

namespace InstaLike.Web.Extensions
{
    public static class ClaimPrincipalExtensions
    {
        public static Guid GetIdentifier(this ClaimsPrincipal principal)
        {
            string? claimValue = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(claimValue))
            {
                throw new InvalidOperationException("NameIdentifier not found. Did you authenticate correctly?");
            }

            return Guid.Parse(claimValue);
        }

        public static MemberId GetMemberID(this ClaimsPrincipal principal)
        {
            return MemberId.From(principal.GetIdentifier());
        }

        public static AccountId GetAccountID(this ClaimsPrincipal principal)
        {
            return AccountId.From(principal.GetIdentifier());
        }
    }
}
