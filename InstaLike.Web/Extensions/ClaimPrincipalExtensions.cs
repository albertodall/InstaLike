using System;
using System.Security.Claims;

namespace InstaLike.Web.Extensions
{
    public static class ClaimPrincipalExtensions
    {
        public static int GetIdentifier(this ClaimsPrincipal principal)
        {
            string claimValue = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(claimValue))
            {
                throw new InvalidOperationException("NameIdentifier not found. Did you authenticate correctly?");
            }
            return int.Parse(claimValue);
        }
    }
}
