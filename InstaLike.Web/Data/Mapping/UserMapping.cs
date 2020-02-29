using FluentNHibernate.Mapping;
using InstaLike.Core.Domain;
using InstaLike.Web.Data.Types;

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
            Map(p => p.ProfilePicture)
                .CustomType<ProfilePictureType>()
                .Columns.Clear()
                .Columns.Add("ProfilePicture").Not.Nullable()
                .Columns.Add("ProfilePictureGuid").Not.Nullable();

            Component(p => p.FullName, m =>
            {
                m.Map(p => p.Name).CustomType<string>().Not.Nullable();
                m.Map(p => p.Surname).CustomType<string>().Not.Nullable();
            });

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
