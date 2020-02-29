using FluentNHibernate.Mapping;
using InstaLike.Core.Domain;
using InstaLike.Web.Data.Types;

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

            Map(memberExpression: p => p.Picture)
                .CustomType<PostPictureType>()
                .Columns.Clear()
                .Columns.Add("[Picture]").Not.Nullable()
                .Columns.Add("PostGuid").Length(2_000_000).Not.Nullable();
                
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