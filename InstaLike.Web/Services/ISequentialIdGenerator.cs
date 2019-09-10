using System;

namespace InstaLike.Web.Services
{
    public interface ISequentialIdGenerator<TId>
    {
        TId GetNextId();
    }

    public interface ISequentialGuidGenerator : ISequentialIdGenerator<Guid> { }
}
