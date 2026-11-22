using MediatR;

namespace InstaLike.Web.Data
{
    /// <summary>
    /// Runs in its own transaction, committed together with the domain events it raises.
    /// A failed result rolls the transaction back, unless the command is marked
    /// with <see cref="CommitOnFailureAttribute"/>.
    /// </summary>
    public interface ICommand<out TResult> : IRequest<TResult>
    {
    }
}
