using FluentNHibernate.Mapping;
using InstaLike.Core.Accounts;
using InstaLike.Web.Data.Types;

namespace InstaLike.Web.Data.Mapping;

internal sealed class AccountMapping : ClassMap<Account>
{
    public AccountMapping()
    {
        Table("[Account]");

        Id(p => p.ID).CustomType<TypedIdType<AccountId>>().GeneratedBy.Assigned();

        Map(p => p.Username).AnsiString(20)
            .Access.CamelCaseField(Prefix.Underscore)
            .Not.Nullable();
        Map(p => p.Password).AnsiString(64)
            .Access.CamelCaseField(Prefix.Underscore)
            .Not.Nullable();
        Map(p => p.Email).AnsiString(50)
            .Access.CamelCaseField(Prefix.Underscore)
            .Not.Nullable();
    }
}
