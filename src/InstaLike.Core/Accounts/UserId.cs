using System;
using System.Diagnostics.CodeAnalysis;
using InstaLike.Core.SharedKernel;
using UUIDNext;

namespace InstaLike.Core.Accounts;

/// <summary>
/// Identifies a user.
/// </summary>
/// <remarks>
/// New identifiers are generated in the application, in the order SQL Server sorts them, so they are known before the
/// user is stored and do not fragment the clustered index.
/// </remarks>
public sealed record UserId(Guid Value) : ITypedId<UserId>
{
    public static UserId New() => new(Uuid.NewDatabaseFriendly(Database.SqlServer));

    public static UserId From(Guid value) => new(value);

    public static UserId Parse(string s, IFormatProvider? provider) => new(Guid.Parse(s, provider));

    public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, [MaybeNullWhen(false)] out UserId result)
    {
        result = Guid.TryParse(s, provider, out var value) ? new UserId(value) : null;
        return result is not null;
    }

    public override string ToString() => Value.ToString();
}
