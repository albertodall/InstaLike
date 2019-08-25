using System;

namespace InstaLike.Web.Data.Mapping
{
    [OnPremDatabaseMapping]
    internal class OnPremUserMapping : UserMapping
    {
        public OnPremUserMapping()
        {
            Component(p => p.ProfilePicture, m =>
            {
                m.Map(p => p.Identifier).CustomType<Guid>()
                    .Column("ProfilePictureGuid")
                    .Not.Insert()
                    .Not.Update();
                m.Map(p => p.RawBytes).CustomType<byte[]>()
                    .Column("ProfilePicture")
                    .Length(100_000)
                    .Not.Nullable();
            });
        }
    }
}
