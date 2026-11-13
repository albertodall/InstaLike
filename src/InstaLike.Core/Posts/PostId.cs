using System;
using System.Diagnostics.CodeAnalysis;
using InstaLike.Core.SharedKernel;
using UUIDNext;

namespace InstaLike.Core.Posts;

/// <summary>
/// Identifies a post.
/// </summary>
/// <remarks>
/// New identifiers are generated in the application, in the order SQL Server sorts them, so they are known before the
/// post is stored and do not fragment the clustered index.
/// </remarks>
public sealed record PostId(Guid Value) : ITypedId<PostId>
{
    public static PostId New() => new(Uuid.NewDatabaseFriendly(Database.SqlServer));

    public static PostId From(Guid value) => new(value);

    public static PostId Parse(string s, IFormatProvider? provider) => new(Guid.Parse(s, provider));

    public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, [MaybeNullWhen(false)] out PostId result)
    {
        result = Guid.TryParse(s, provider, out var value) ? new PostId(value) : null;
        return result is not null;
    }

    public override string ToString() => Value.ToString();
}
