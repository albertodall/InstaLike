using System;
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
                    await tx.RollbackAsync(CancellationToken.None);
                    await _unitOfWork.RolledBackAsync();
                    return response;
                }

                await DispatchDomainEventsAsync(cancellationToken);
                await tx.CommitAsync(cancellationToken);
            }
            catch
            {
                if (tx.IsActive)
                {
                    await tx.RollbackAsync(CancellationToken.None);
                }

                await _unitOfWork.RolledBackAsync();
                throw;
            }
        }

        await _unitOfWork.CommittedAsync();
        return response;
    }

    private async Task DispatchDomainEventsAsync(CancellationToken cancellationToken)
    {
        while (true)
        {
            var entities = _session.GetSessionImplementation().PersistenceContext.EntitiesByKey.Values
                .OfType<IAggregateRoot>()
                .Where(e => e.DomainEvents.Count > 0)
                .ToList();
            if (entities.Count == 0)
            {
                return;
            }

            var domainEvents = entities.SelectMany(e => e.DomainEvents).ToList();
            entities.ForEach(e => e.ClearDomainEvents());

            foreach (var domainEvent in domainEvents)
            {
                await _publisher.Publish(domainEvent, cancellationToken);
            }

            await _session.FlushAsync(cancellationToken);
        }
    }
}
