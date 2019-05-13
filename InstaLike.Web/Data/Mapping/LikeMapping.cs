using System;
using FluentNHibernate.Mapping;
using InstaLike.Core.Domain;

namespace InstaLike.Web.Data.Mapping
{
    internal class LikeMapping : ClassMap<Like>
    {
        public LikeMapping()
        {
            Table("[Like]");

            Id(p => p.ID).GeneratedBy.Native();

            Map(p => p.LikeDate)
                .Not.Nullable();

            References(p => p.Post)
                .Column("PostID");

            References(p => p.User)
                .Column("UserID");
        }
    }
}