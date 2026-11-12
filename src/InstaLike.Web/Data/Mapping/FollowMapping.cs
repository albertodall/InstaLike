using FluentNHibernate.Mapping;
using InstaLike.Core.Social;

namespace InstaLike.Web.Data.Mapping;

internal sealed class FollowMapping : ClassMap<Follow>
{
    public FollowMapping()
    {
        Table("[Follow]");

        Id(p => p.ID).GeneratedBy.Native();

        Map(p => p.FollowDate)
            .Not.Nullable();

        Map(p => p.FollowerID)
            .Not.Nullable();

        Map(p => p.FollowedID)
            .Not.Nullable();
    }
}
