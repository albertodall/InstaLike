using FluentNHibernate.Mapping;
using InstaLike.Core.Posts;

namespace InstaLike.Web.Data.Mapping;

internal sealed class CommentMapping : ClassMap<Comment>
{
    public CommentMapping()
    {
        Table("[Comment]");

        Id(p => p.ID).GeneratedBy.Native();

        Map(p => p.Text).AnsiString(500)
            .Access.CamelCaseField(Prefix.Underscore)
            .Not.Nullable();

        Map(p => p.CommentDate)
            .Not.Nullable();

        References(p => p.Post)
            .Column("PostID");

        Map(p => p.AuthorID)
            .Column("UserID")
            .Not.Nullable();
    }
}
