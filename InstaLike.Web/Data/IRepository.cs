using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using InstaLike.Core.Domain;

namespace InstaLike.Web.Data
{
    public interface IRepository<TEntity, TKey>
        where TEntity : EntityBase<TKey>
    {
        Task<Maybe<TEntity>> GetById(TKey id);
        Task<Result<TKey>> Save(TEntity entity);
    }
}
