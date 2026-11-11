using System;
using System.Threading.Tasks;

namespace InstaLike.Web.Data;

public interface IUnitOfWork
{
    void OnCommitted(Func<Task> action);

    void OnRolledBack(Func<Task> action);
}
