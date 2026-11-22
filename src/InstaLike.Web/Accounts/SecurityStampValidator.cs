using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using InstaLike.Core.Accounts;
using InstaLike.Web.Extensions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using NHibernate;

namespace InstaLike.Web.Accounts
{
    /// <summary>
    /// Signs out the requests whose cookie carries a security stamp the account no longer has,
    /// so that changing the password logs out every other session.
    /// </summary>
    internal sealed class SecurityStampValidator : CookieAuthenticationEvents
    {
        public const string ClaimType = "InstaLike.SecurityStamp";

        private readonly IStatelessSession _session;

        public SecurityStampValidator(IStatelessSession session)
        {
            _session = session;
        }

        public override async Task ValidatePrincipal(CookieValidatePrincipalContext context)
        {
            if (context.Principal is not null && await HasCurrentSecurityStampAsync(context.Principal, context.HttpContext.RequestAborted))
            {
                return;
            }

            context.RejectPrincipal();
            await context.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        }

        private async Task<bool> HasCurrentSecurityStampAsync(ClaimsPrincipal principal, CancellationToken cancellationToken)
        {
            var currentSecurityStamp = await ReadSecurityStampAsync(principal.GetAccountID(), cancellationToken);

            return currentSecurityStamp is not null
                && principal.FindFirst(ClaimType)?.Value == currentSecurityStamp.Value.ToString();
        }

        private async Task<Guid?> ReadSecurityStampAsync(AccountId accountID, CancellationToken cancellationToken)
        {
            using var tx = _session.BeginTransaction();

            var securityStamp = await _session.QueryOver<Account>()
                .Where(a => a.ID == accountID)
                .Select(a => a.SecurityStamp)
                .SingleOrDefaultAsync<Guid?>(cancellationToken);

            await tx.CommitAsync(cancellationToken);
            return securityStamp;
        }
    }
}
