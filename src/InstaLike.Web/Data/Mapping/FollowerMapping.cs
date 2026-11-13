using FluentNHibernate.Mapping;
using InstaLike.Core.Social;
using InstaLike.Web.Data.Types;

namespace InstaLike.Web.Data.Mapping
{
    internal sealed class FollowerMapping : ClassMap<Follower>
    {
        public FollowerMapping()
        {
            Table("[Follower]");

            Id(p => p.ID).CustomType<TypedIdType<FollowerId>>().GeneratedBy.Assigned();

            HasMany(p => p.Follows)
                .Access.CamelCaseField(Prefix.Underscore)
                .KeyColumn("FollowerID");
        }
    }
}
