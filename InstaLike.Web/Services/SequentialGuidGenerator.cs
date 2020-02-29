using System;
using RT.Comb;

namespace InstaLike.Web.Services
{
    internal class SequentialGuidGenerator : ISequentialGuidGenerator
    {
        public Guid GetNextId() => Provider.Sql.Create();
    }
}
