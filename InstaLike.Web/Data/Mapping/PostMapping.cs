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

            Map(p => p.Text).CustomType<string>().Not.Nullable();
            Map(p => p.PostDate).CustomType<DateTimeOffset>().Not.Nullable();

            Component(p => p.Picture, m => 
            {
                m.Map(p => p.Identifier).CustomType<Guid>()
                    .Column("PostGuid")
                    .Not.Insert()
                    .Not.Update()
                    .Not.Nullable();
                m.Map(p => p.RawBytes).CustomType<byte[]>()
                    .Not.Nullable();
            });

            References(p => p.Author).Not.Nullable();

            HasMany(p => p.Comments);
            HasMany(p => p.Likes);

            DynamicInsert();
        }
    }
}
