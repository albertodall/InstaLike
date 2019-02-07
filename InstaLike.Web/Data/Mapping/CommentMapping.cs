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

            Map(p => p.Text).CustomType<string>().Not.Nullable();

            References(p => p.Post);
            References(p => p.User);

            DynamicInsert();
        }
    }
}
