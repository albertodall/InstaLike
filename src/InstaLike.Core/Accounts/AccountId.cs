using System;
using System.Diagnostics.CodeAnalysis;
using InstaLike.Core.SharedKernel;

namespace InstaLike.Core.Accounts
{
    /// <summary>
    /// Identifies an account.
    /// </summary>
    /// <remarks>
    /// An account shares the identifier of the member it belongs to, so it is never generated on its own.
    /// </remarks>
    public sealed record AccountId(Guid Value) : ITypedId<AccountId>
    {
        public static AccountId From(Guid value) => new(value);

        public static AccountId Parse(string s, IFormatProvider? provider) => new(Guid.Parse(s, provider));

        public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, [MaybeNullWhen(false)] out AccountId result)
        {
            result = Guid.TryParse(s, provider, out var value) ? new AccountId(value) : null;
            return result is not null;
        }

        public override string ToString() => Value.ToString();
    }
}
