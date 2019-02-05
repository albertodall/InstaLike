﻿using System;
using FluentNHibernate.Mapping;
using InstaLike.Core.Domain;

namespace InstaLike.Web.Data.Mapping
{
    internal class UserMapping : ClassMap<User>
    {
        public UserMapping()
        {
            Table("User");

            Id(p => p.ID).GeneratedBy.Native();

            Map(p => p.Biography).CustomType<string>()
                .Not.Nullable();
            Map(p => p.Email).CustomType<string>()
                .Access.CamelCaseField(Prefix.Underscore)
                .Not.Nullable();
            Map(p => p.FirstName).CustomType<string>()
                .Not.Nullable();
            Map(p => p.LastName).CustomType<string>()
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
                    .Generated.Never()
                    .Not.Nullable();
                m.Map(p => p.RawBytes).CustomType<byte[]>()
                    .Not.Nullable();
            });

            HasMany(p => p.Followers);
            HasMany(p => p.Following);

            DynamicInsert();
        }
    }
}
