using System;
using FluentNHibernate.Mapping;
using InstaLike.Core.Domain;

namespace InstaLike.Web.Data.Mapping
{
    internal class LikeMapping : ClassMap<Like>
    {
        public LikeMapping()
        {
            Table("[Like]");

            Id(p => p.ID).GeneratedBy.Native();

            Map(p => p.LikeDate).CustomType<DateTimeOffset>()
                .Not.Nullable();

            References(p => p.Post).Not.Nullable();
            References(p => p.User).Not.Nullable();
        }
    }
}
