using System;
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
            Map(p => p.CommentDate).CustomType<DateTimeOffset>().Not.Nullable();

            References(p => p.Post)
                .Column("PostID")
                .Not.Nullable();

            References(p => p.User)
                .Column("UserID")
                .Not.Nullable();

            DynamicInsert();
        }
    }
}
