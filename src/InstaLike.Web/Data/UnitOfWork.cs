using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using InstaLike.Core.SharedKernel;
using MediatR;
using NHibernate;

namespace InstaLike.Web.Data;

internal sealed class UnitOfWork
{
    private readonly ISession _session;
    private readonly IPublisher _publisher;
    private ITransaction? _transaction;

    public UnitOfWork(ISession session, IPublisher publisher)
    {
        _session = session;
        _publisher = publisher;
    }

    private ITransaction Transaction =>
        _transaction ?? throw new InvalidOperationException("The unit of work has not begun.");

    public void Begin() => _transaction = _session.BeginTransaction();

    public async Task CommitAsync(CancellationToken cancellationToken)
    {
        await DispatchDomainEventsAsync(cancellationToken);
        await Transaction.CommitAsync(cancellationToken);
    }

    public async Task RollbackAsync()
    {
        using var transaction = Transaction;
        if (transaction.IsActive)
        {
            await transaction.RollbackAsync(CancellationToken.None);
        }

        AggregatesWithPendingEvents().ForEach(a => a.ClearDomainEvents());
        _session.Clear();
    }

    private async Task DispatchDomainEventsAsync(CancellationToken cancellationToken)
    {
        while (true)
        {
            var aggregates = AggregatesWithPendingEvents();
            if (aggregates.Count == 0)
            {
                return;
            }

            var domainEvents = aggregates.SelectMany(a => a.DomainEvents).ToList();
            aggregates.ForEach(a => a.ClearDomainEvents());

            foreach (var domainEvent in domainEvents)
            {
                await _publisher.Publish(domainEvent, cancellationToken);
            }

            await _session.FlushAsync(cancellationToken);
        }
    }

    private List<IAggregateRoot> AggregatesWithPendingEvents()
    {
        return _session.GetSessionImplementation().PersistenceContext.EntitiesByKey.Values
            .OfType<IAggregateRoot>()
            .Where(a => a.DomainEvents.Count > 0)
            .ToList();
    }
}
