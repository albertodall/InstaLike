using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using InstaLike.Core.Commands;
using InstaLike.Web.Data.Query;

namespace InstaLike.Web.Infrastructure
{
    public interface IMessageDispatcher
    {
        Task<Result> DispatchAsync(ICommand command);
        Task<TResult> DispatchAsync<TResult>(IQuery<TResult> query);
    }
}