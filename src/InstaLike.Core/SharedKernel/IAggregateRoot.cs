using System.Collections.Generic;

namespace InstaLike.Core.SharedKernel;

/// <summary>
/// Represents the root of an aggregate and exposes its domain events.
/// </summary>
/// <remarks>
/// Domain events remain available until they are cleared.
/// </remarks>
public interface IAggregateRoot
{
    IReadOnlyCollection<IDomainEvent> DomainEvents { get; }

    void ClearDomainEvents();
}
