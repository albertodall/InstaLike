using System;

namespace InstaLike.Core.SharedKernel;

/// <summary>
/// Stored pictures are never overwritten: new content always gets a new identifier.
/// </summary>
public sealed record PictureId(Guid Value)
{
    public static PictureId New() => new(Guid.NewGuid());
}
