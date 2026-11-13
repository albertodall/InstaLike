using FluentNHibernate.Mapping;
using InstaLike.Core.Social;

namespace InstaLike.Web.Data.Mapping;

internal sealed class FollowMapping : ClassMap<Follow>
{
    public FollowMapping()
    {
        Table("[Follow]");

        Id(p => p.ID).GeneratedBy.GuidComb();

        Map(p => p.FollowDate)
            .Not.Nullable();

        References(p => p.Follower)
            .Column("FollowerID");

        Map(p => p.FollowedID)
            .Not.Nullable();
    }
}
