using FluentNHibernate.Mapping;
using InstaLike.Core.Posts;
using InstaLike.Web.Data.Types;

namespace InstaLike.Web.Data.Mapping;

internal class PostMapping : ClassMap<Post>
{
    public PostMapping()
    {
        Table("[Post]");

        Id(p => p.ID).GeneratedBy.Native();

        Map(p => p.Text).CustomType<string>()
            .Access.CamelCaseField(Prefix.Underscore)
            .Not.Nullable();

        Map(p => p.PostDate)
            .Not.Nullable();

        Map(p => p.Picture)
            .CustomType<PictureIdType>()
            .Column("PostGuid")
            .Not.Nullable();

        References(p => p.Author)
            .Column("UserID");

        HasMany(p => p.Comments)
            .KeyColumn("PostID")
            .Access.CamelCaseField(Prefix.Underscore);

        HasMany(p => p.Likes)
            .KeyColumn("PostID")
            .Access.CamelCaseField(Prefix.Underscore);

        DynamicInsert();
    }
}
