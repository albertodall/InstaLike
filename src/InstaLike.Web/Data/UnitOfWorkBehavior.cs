using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using MediatR;

namespace InstaLike.Web.Data;

internal sealed class UnitOfWorkBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly UnitOfWork _unitOfWork;

    public UnitOfWorkBehavior(UnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (request is not ICommand<TResponse>)
        {
            return await next(cancellationToken);
        }

        _unitOfWork.Begin();
        try
        {
            var response = await next(cancellationToken);
            if (response is IResult { IsFailure: true })
            {
                await _unitOfWork.RollbackAsync();
                return response;
            }

            await _unitOfWork.CommitAsync(cancellationToken);
            return response;
        }
        catch
        {
            await _unitOfWork.RollbackAsync();
            throw;
        }
    }
}
