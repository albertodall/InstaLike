using System;
using FluentNHibernate.Mapping;
using InstaLike.Core.Domain;

namespace InstaLike.Web.Data.Mapping
{
    internal class UserMapping : ClassMap<User>
    {
        public UserMapping()
        {
            Table("[User]");

            Id(p => p.ID).GeneratedBy.Native();

            Map(p => p.Biography).CustomType<string>()
                .Not.Nullable()
                .LazyLoad();
            Map(p => p.Email).CustomType<string>()
                .Access.CamelCaseField(Prefix.Underscore)
                .Not.Nullable();           
            Map(p => p.Nickname).CustomType<string>()
                .Access.CamelCaseField(Prefix.Underscore)
                .Not.Nullable();
            Map(p => p.Password).CustomType<string>()
                .Access.CamelCaseField(Prefix.Underscore)
                .Not.Nullable();
            Map(p => p.RegistrationDate)
                .Not.Nullable();

            Component(p => p.FullName, m =>
            {
                m.Map(p => p.Name).CustomType<string>().Not.Nullable();
                m.Map(p => p.Surname).CustomType<string>().Not.Nullable();
            });

            Component(p => p.ProfilePicture, m =>
            {
                m.Map(p => p.Identifier).CustomType<Guid>()
                    .Column("ProfilePictureGuid")
                    .Not.Insert()
                    .Not.Update();
                m.Map(p => p.RawBytes).CustomType<byte[]>()
                    .Column("ProfilePicture")
                    .Length(100_000)
                    .Not.Nullable();
            }).LazyLoad();

            HasMany(p => p.Followers)
                .KeyColumn("FollowedID")
                .Access.CamelCaseField(Prefix.Underscore);

            HasMany(p => p.Followed)
                .KeyColumn("FollowerID")
                .Access.CamelCaseField(Prefix.Underscore);

            DynamicInsert();
        }
    }
}
