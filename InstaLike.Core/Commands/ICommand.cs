using System.Threading.Tasks;
using CSharpFunctionalExtensions;

namespace InstaLike.Core.Commands
{
    public interface ICommand
    {
    }

    public interface ICommandHandler<TCommand>
        where TCommand : ICommand
    {
        Task<Result> HandleAsync(TCommand command);
    }
}
