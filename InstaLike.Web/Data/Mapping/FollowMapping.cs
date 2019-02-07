using System;
using FluentNHibernate.Mapping;
using InstaLike.Core.Domain;

namespace InstaLike.Web.Data.Mapping
{
    internal class FollowMapping : ClassMap<Follow>
    {
        public FollowMapping()
        {
            Table("[Follow]");

            Id(p => p.ID).GeneratedBy.Native();
            Map(p => p.FollowDate).CustomType<DateTimeOffset>().Not.Nullable();

            References(p => p.Follower).Not.Nullable();
            References(p => p.Following).Not.Nullable();
        }
    }
}