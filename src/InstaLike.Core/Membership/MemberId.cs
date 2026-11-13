using System;
using System.Diagnostics.CodeAnalysis;
using InstaLike.Core.SharedKernel;
using UUIDNext;

namespace InstaLike.Core.Membership;

/// <summary>
/// Identifies a member.
/// </summary>
/// <remarks>
/// New identifiers are generated in the application, in the order SQL Server sorts them, so they are known before the
/// member is stored and do not fragment the clustered index.
/// </remarks>
public sealed record MemberId(Guid Value) : ITypedId<MemberId>
{
    public static MemberId New() => new(Uuid.NewDatabaseFriendly(Database.SqlServer));

    public static MemberId From(Guid value) => new(value);

    public static MemberId Parse(string s, IFormatProvider? provider) => new(Guid.Parse(s, provider));

    public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, [MaybeNullWhen(false)] out MemberId result)
    {
        result = Guid.TryParse(s, provider, out var value) ? new MemberId(value) : null;
        return result is not null;
    }

    public override string ToString() => Value.ToString();
}
