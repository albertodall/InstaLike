using FluentNHibernate.Mapping;
using InstaLike.Core.Membership;
using InstaLike.Core.SharedKernel;
using InstaLike.Web.Data.Types;

namespace InstaLike.Web.Data.Mapping
{
    internal sealed class MemberMapping : ClassMap<Member>
    {
        public MemberMapping()
        {
            Table("[Member]");

            Id(p => p.ID).CustomType<TypedIdType<MemberId>>().GeneratedBy.Assigned();

            Map(p => p.Biography).AnsiString(500)
                .Not.Nullable()
                .LazyLoad();
            Map(p => p.Nickname).AnsiString(20)
                .Access.CamelCaseField(Prefix.Underscore)
                .Not.Nullable();
            Map(p => p.RegistrationDate)
                .Not.Nullable();
            Map(p => p.ProfilePicture)
                .CustomType<TypedIdType<PictureId>>()
                .Column("ProfilePictureGuid")
                .Nullable();

            Component(p => p.FullName, m =>
            {
                m.Map(p => p.Name).AnsiString(30).Not.Nullable();
                m.Map(p => p.Surname).AnsiString(50).Not.Nullable();
            });
        }
    }
}
