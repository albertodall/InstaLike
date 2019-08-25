using System;

namespace InstaLike.Web.Data.Mapping.OnPrem
{
    [OnPremDatabaseMapping]
    internal class OnPremPostMapping : PostMapping
    {
        public OnPremPostMapping()
        {
            Component(p => p.Picture, m =>
            {
                m.Map(p => p.Identifier).CustomType<Guid>()
                    .Column("PostGuid")
                    .Not.Insert()
                    .Not.Update();
                m.Map(p => p.RawBytes)
                    .Column("Picture")
                    .CustomType<byte[]>()
                    .Length(2_000_000)
                    .Not.Nullable();
            });
        }
    }
}
