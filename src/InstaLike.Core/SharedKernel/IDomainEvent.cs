using MediatR;

namespace InstaLike.Core.SharedKernel
{
    /// <summary>
    /// Represents an event that occurred within the domain.
    /// </summary>
    public interface IDomainEvent : INotification
    {
    }
}
