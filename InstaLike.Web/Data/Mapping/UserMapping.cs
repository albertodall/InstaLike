using System;
using FluentNHibernate;
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
                .Not.Nullable();
            Map(p => p.Email).CustomType<string>()
                .Access.CamelCaseField(Prefix.Underscore)
                .Not.Nullable();
            Map(p => p.Name).CustomType<string>()
                .Not.Nullable();
            Map(p => p.Surname).CustomType<string>()
                .Not.Nullable();
            Map(p => p.Nickname).CustomType<string>()
                .Access.CamelCaseField(Prefix.Underscore)
                .Not.Nullable();
            Map(p => p.Password).CustomType<string>()
                .Access.CamelCaseField(Prefix.Underscore)
                .Not.Nullable();
            Map(p => p.RegistrationDate).CustomType<DateTimeOffset>()
                .Not.Nullable();

            Component(p => p.ProfilePicture, m =>
            {
                m.Map(p => p.Identifier).CustomType<Guid>()
                    .Not.Insert()
                    .Not.Update();
                m.Map(p => p.RawBytes).CustomType<byte[]>()
                    .Column("ProfilePicture")
                    .Not.Nullable();
            });

            HasMany(p => p.Followers).KeyColumn("ID");
            HasMany(p => p.Following).KeyColumn("ID");

            DynamicInsert();
        }
    }
}
