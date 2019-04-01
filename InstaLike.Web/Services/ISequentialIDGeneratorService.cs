using System;

namespace InstaLike.Web.Services
{
    internal interface ISequentialIDGeneratorService<out TID>
    {
        TID GetIdentifier();
    }

    internal interface ISequentialGuidGeneratorService : ISequentialIDGeneratorService<Guid>
    { }
}
