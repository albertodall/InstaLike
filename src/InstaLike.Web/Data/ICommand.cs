using MediatR;

namespace InstaLike.Web.Data;

/// <summary>
/// Runs in its own transaction, committed together with the domain events it raises.
/// </summary>
public interface ICommand<out TResult> : IRequest<TResult>
{
}
