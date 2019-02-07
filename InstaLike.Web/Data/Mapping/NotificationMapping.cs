using FluentNHibernate.Mapping;
using InstaLike.Core.Domain;

namespace InstaLike.Web.Data.Mapping
{
    internal class NotificationMapping : ClassMap<Notification>
    {
        public NotificationMapping()
        {
            Table("[Notification]");

            Id(p => p.ID).GeneratedBy.Native();

            Map(p => p.IsRead).CustomType<bool>().Not.Nullable();
            Map(p => p.Message).CustomType<string>().Not.Nullable();

            References(p => p.Recipient);
            References(p => p.Sender);

            DynamicInsert();
        }
    }
}
