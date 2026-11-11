using FluentNHibernate.Mapping;
using InstaLike.Core.Posts;

namespace InstaLike.Web.Data.Mapping;

internal class LikeMapping : ClassMap<Like>
{
    public LikeMapping()
    {
        Table("[Like]");

        Id(p => p.ID).GeneratedBy.Native();

        Map(p => p.LikeDate)
            .Not.Nullable();

        Map(p => p.PostID)
            .Not.Nullable();

        Map(p => p.UserID)
            .Not.Nullable();
    }
}
