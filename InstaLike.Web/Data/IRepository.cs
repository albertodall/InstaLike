using System.Threading.Tasks;
using InstaLike.Core.Domain;

namespace InstaLike.Web.Data
{
    public interface IRepository<TEntity, TKey>
        where TEntity : EntityBase<TKey>
    {
        Task<TEntity> GetById(TKey id);
        Task Save(TEntity entity);
    }
}
