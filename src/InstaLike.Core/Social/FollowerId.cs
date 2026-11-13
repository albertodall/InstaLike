using System;
using System.Diagnostics.CodeAnalysis;
using InstaLike.Core.SharedKernel;

namespace InstaLike.Core.Social;

/// <summary>
/// Identifies a follower.
/// </summary>
/// <remarks>
/// A follower shares the identifier of the member it represents, so it is never generated on its own.
/// </remarks>
public sealed record FollowerId(Guid Value) : ITypedId<FollowerId>
{
    public static FollowerId From(Guid value) => new(value);

    public static FollowerId Parse(string s, IFormatProvider? provider) => new(Guid.Parse(s, provider));

    public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, [MaybeNullWhen(false)] out FollowerId result)
    {
        result = Guid.TryParse(s, provider, out var value) ? new FollowerId(value) : null;
        return result is not null;
    }

    public override string ToString() => Value.ToString();
}
