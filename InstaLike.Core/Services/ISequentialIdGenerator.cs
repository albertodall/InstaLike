using System;

namespace InstaLike.Core.Services
{
    public interface ISequentialIdGenerator<out TId>
    {
        TId GetNextId();
    }

    public interface ISequentialGuidGenerator : ISequentialIdGenerator<Guid> { }
}
