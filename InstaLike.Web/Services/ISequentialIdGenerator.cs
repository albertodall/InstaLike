using System;

namespace InstaLike.Web.Services
{
    public interface ISequentialIdGenerator<out TId>
    {
        TId GetNextId();
    }

    public interface ISequentialGuidGenerator : ISequentialIdGenerator<Guid> { }
}
