using FluentNHibernate.Mapping;
using InstaLike.Core.Posts;
using InstaLike.Web.Data.Types;

namespace InstaLike.Web.Data.Mapping;

internal sealed class PostMapping : ClassMap<Post>
{
    public PostMapping()
    {
        Table("[Post]");

        Id(p => p.ID).GeneratedBy.Native();

        Map(p => p.Text).AnsiString(500)
            .Access.CamelCaseField(Prefix.Underscore)
            .Not.Nullable();

        Map(p => p.PostDate)
            .Not.Nullable();

        Map(p => p.Picture)
            .CustomType<PictureIdType>()
            .Column("PostGuid")
            .Not.Nullable();

        Map(p => p.AuthorID)
            .Column("UserID")
            .Not.Nullable();

        HasMany(p => p.Likes)
            .Access.CamelCaseField(Prefix.Underscore)
            .KeyColumn("PostID");

        HasMany(p => p.Comments)
            .Access.CamelCaseField(Prefix.Underscore)
            .KeyColumn("PostID");
    }
}
