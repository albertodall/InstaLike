using FluentNHibernate.Mapping;
using InstaLike.Core.Posts;

namespace InstaLike.Web.Data.Mapping;

internal class CommentMapping : ClassMap<Comment>
{
    public CommentMapping()
    {
        Table("[Comment]");

        Id(p => p.ID).GeneratedBy.Native();

        Map(p => p.Text).CustomType<string>()
            .Access.CamelCaseField(Prefix.Underscore)
            .Not.Nullable();

        Map(p => p.CommentDate)
            .Not.Nullable();

        Map(p => p.PostID)
            .Not.Nullable();

        Map(p => p.AuthorID)
            .Column("UserID")
            .Not.Nullable();

        DynamicInsert();
    }
}
