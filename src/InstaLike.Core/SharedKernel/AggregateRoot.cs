using System.Collections.Generic;

namespace InstaLike.Core.SharedKernel;

/// <summary>
/// A base class for aggregate roots that collect domain events.
/// </summary>
/// <remarks>
/// Domain events raised by the aggregate root are available through <c>DomainEvents</c> until
/// cleared.
/// </remarks>
/// <typeparam name="TId">The non-nullable type of the aggregate root's identifier.</typeparam>
public abstract class AggregateRoot<TId> : EntityBase<TId>, IAggregateRoot
    where TId : notnull
{
    private readonly List<IDomainEvent> _domainEvents = [];

    protected AggregateRoot() { }

    protected AggregateRoot(TId id)
        : base(id)
    {
    }

    public virtual IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    public virtual void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }

    protected virtual void RaiseDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }
}
