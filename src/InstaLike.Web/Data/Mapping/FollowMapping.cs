using FluentNHibernate.Mapping;
using InstaLike.Core.Social;

namespace InstaLike.Web.Data.Mapping;

internal class FollowMapping : ClassMap<Follow>
{
    public FollowMapping()
    {
        Table("[Follow]");

        Id(p => p.ID).GeneratedBy.Native();

        Map(p => p.FollowDate)
            .Not.Nullable();

        References(p => p.Follower)
            .Column("FollowerID");

        References(p => p.Followed)
            .Column("FollowedID");
    }
}
