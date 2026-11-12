using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using InstaLike.Core.SharedKernel;
using MediatR;
using NHibernate;

namespace InstaLike.Web.Data;

internal sealed class UnitOfWorkBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ISession _session;
    private readonly IPublisher _publisher;
    private readonly UnitOfWork _unitOfWork;

    public UnitOfWorkBehavior(ISession session, IPublisher publisher, UnitOfWork unitOfWork)
    {
        _session = session ?? throw new ArgumentNullException(nameof(session));
        _publisher = publisher ?? throw new ArgumentNullException(nameof(publisher));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (request is not ICommand<TResponse>)
        {
            return await next(cancellationToken);
        }

        TResponse response;
        using (var tx = _session.BeginTransaction())
        {
            try
            {
                response = await next(cancellationToken);
                if (response is IResult { IsFailure: true })
                {
                    await RollbackAsync(tx);
                    return response;
                }

                await DispatchDomainEventsAsync(cancellationToken);
                await tx.CommitAsync(cancellationToken);
            }
            catch
            {
                await RollbackAsync(tx);
                throw;
            }
        }

        await _unitOfWork.CommittedAsync();
        return response;
    }

    private async Task RollbackAsync(ITransaction tx)
    {
        if (tx.IsActive)
        {
            await tx.RollbackAsync(CancellationToken.None);
        }

        DiscardRolledBackChanges();
        await _unitOfWork.RolledBackAsync();
    }

    private void DiscardRolledBackChanges()
    {
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
