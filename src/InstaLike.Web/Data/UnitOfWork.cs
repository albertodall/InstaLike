using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace InstaLike.Web.Data;

internal sealed class UnitOfWork : IUnitOfWork
{
    private readonly List<Func<Task>> _onCommitted = new();
    private readonly List<Func<Task>> _onRolledBack = new();

    public void OnCommitted(Func<Task> action) => _onCommitted.Add(action);

    public void OnRolledBack(Func<Task> action) => _onRolledBack.Add(action);

    public Task CommittedAsync() => RunAndClearAsync(_onCommitted);

    public Task RolledBackAsync() => RunAndClearAsync(_onRolledBack);

    private async Task RunAndClearAsync(List<Func<Task>> actions)
    {
        var toRun = actions.ToArray();
        _onCommitted.Clear();
        _onRolledBack.Clear();

        foreach (var action in toRun)
        {
            await action();
        }
    }
}
