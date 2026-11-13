using FluentNHibernate.Mapping;
using InstaLike.Core.Posts;

namespace InstaLike.Web.Data.Mapping;

internal sealed class LikeMapping : ClassMap<Like>
{
    public LikeMapping()
    {
        Table("[Like]");

        Id(p => p.ID).GeneratedBy.GuidComb();

        Map(p => p.LikeDate)
            .Not.Nullable();

        References(p => p.Post)
            .Column("PostID");

        Map(p => p.MemberID)
            .Not.Nullable();
    }
}
