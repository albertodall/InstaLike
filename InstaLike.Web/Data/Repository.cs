using System;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using InstaLike.Core.Domain;
using NHibernate;

namespace InstaLike.Web.Data
{
    internal class Repository<TEntity, TKey> : IRepository<TEntity, TKey>
        where TEntity : EntityBase<TKey>
    {
        private readonly ISession _session;

        public Repository(ISession session)
        {
            _session = session ?? throw new ArgumentNullException(nameof(session));
        }

        public async Task<Maybe<TEntity>> GetById(TKey id)
        {
            using (var tx = _session.BeginTransaction())
            {
                return Maybe<TEntity>.From(await _session.GetAsync<TEntity>(id));
            }
        }

        public async Task<Result<TKey>> Save(TEntity entity)
        {
            using (var tx = _session.BeginTransaction())
            {
                try
                {
                    await _session.SaveOrUpdateAsync(entity);
                    await tx.CommitAsync();
                    return Result.Ok(entity.ID);
                }
                catch (ADOException ex)
                {
                    await tx.RollbackAsync();
                    return Result.Fail<TKey>(ex.Message);
                }
            }
        }
    }
}
