using System;
using System.Diagnostics.CodeAnalysis;

namespace InstaLike.Core.SharedKernel;

/// <summary>
/// Stored pictures are never overwritten: new content always gets a new identifier.
/// </summary>
public sealed record PictureId(Guid Value) : ITypedId<PictureId>
{
    public static PictureId New() => new(Guid.NewGuid());

    public static PictureId From(Guid value) => new(value);

    public static PictureId Parse(string s, IFormatProvider? provider) => new(Guid.Parse(s, provider));

    public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, [MaybeNullWhen(false)] out PictureId result)
    {
        result = Guid.TryParse(s, provider, out var value) ? new PictureId(value) : null;
        return result is not null;
    }

    public override string ToString() => Value.ToString();
}
