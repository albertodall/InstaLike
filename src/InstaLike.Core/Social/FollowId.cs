using System;
using System.Diagnostics.CodeAnalysis;
using InstaLike.Core.SharedKernel;
using UUIDNext;

namespace InstaLike.Core.Social;

/// <summary>
/// Identifies a follow relationship.
/// </summary>
/// <remarks>
/// New identifiers are generated in the application, in the order SQL Server sorts them, so they are known before the
/// follow relationship is stored and do not fragment the clustered index.
/// </remarks>
public sealed record FollowId(Guid Value) : ITypedId<FollowId>
{
    public static FollowId New() => new(Uuid.NewDatabaseFriendly(Database.SqlServer));

    public static FollowId From(Guid value) => new(value);

    public static FollowId Parse(string s, IFormatProvider? provider) => new(Guid.Parse(s, provider));

    public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, [MaybeNullWhen(false)] out FollowId result)
    {
        result = Guid.TryParse(s, provider, out var value) ? new FollowId(value) : null;
        return result is not null;
    }

    public override string ToString() => Value.ToString();
}
