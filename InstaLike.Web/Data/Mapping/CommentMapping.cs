using FluentNHibernate.Mapping;
using InstaLike.Core.Domain;

namespace InstaLike.Web.Data.Mapping
{
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

            References(p => p.Post)
                .Column("PostID");

            References(p => p.Author)
                .Column("UserID");

            DynamicInsert();
        }
    }
}
