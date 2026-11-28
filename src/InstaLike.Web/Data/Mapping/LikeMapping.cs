using FluentNHibernate.Mapping;
using InstaLike.Core.Posts;
using InstaLike.Core.SharedKernel;
using InstaLike.Web.Data.Types;

namespace InstaLike.Web.Data.Mapping
{
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
                .CustomType<TypedIdType<MemberId>>()
                .Not.Nullable();
        }
    }
}
