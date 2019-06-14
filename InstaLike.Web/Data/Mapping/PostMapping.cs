using System;
using FluentNHibernate.Mapping;
using InstaLike.Core.Domain;

namespace InstaLike.Web.Data.Mapping
{
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

            Component(p => p.Picture, m => 
            {
                m.Map(p => p.Identifier).CustomType<Guid>()
                    .Column("PostGuid")
                    .Not.Insert()
                    .Not.Update();
                m.Map(p => p.RawBytes)
                    .Column("Picture")
                    .CustomType<byte[]>()
                    .Length(2_000_000)
                    .Not.Nullable();
            });

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
}
