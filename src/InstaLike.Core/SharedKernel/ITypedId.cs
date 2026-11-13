using System;

namespace InstaLike.Core.SharedKernel;

/// <summary>
/// An identifier that wraps a <see cref="Guid"/> so that identifiers of different entities cannot be mixed up.
/// </summary>
/// <remarks>
/// Identifiers are parsable from their <see cref="Guid"/> text, so they can be bound from routes and forms.
/// </remarks>
/// <typeparam name="TSelf">The identifier type itself.</typeparam>
public interface ITypedId<TSelf> : IParsable<TSelf>
    where TSelf : ITypedId<TSelf>
{
    Guid Value { get; }

    static abstract TSelf From(Guid value);
}
