using FluentNHibernate.Mapping;
using InstaLike.Core.Social;
using InstaLike.Web.Data.Types;

namespace InstaLike.Web.Data.Mapping;

internal sealed class FollowMapping : ClassMap<Follow>
{
    public FollowMapping()
    {
        Table("[Follow]");

        Id(p => p.ID).CustomType<TypedIdType<FollowId>>().GeneratedBy.Assigned();

        Map(p => p.FollowDate)
            .Not.Nullable();

        Map(p => p.FollowerID)
            .Not.Nullable();

        Map(p => p.FollowedID)
            .Not.Nullable();
    }
}
