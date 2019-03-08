using System;
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

            Map(p => p.HasBeenReadByRecipient).CustomType<bool>()
                .Column("[ReadByRecipient]")
                .Not.Nullable();

            Map(p => p.Message).CustomType<string>()
                .Not.Nullable();

            Map(p => p.NotificationDate).CustomType<DateTimeOffset>()
                .Not.Nullable();

            References(p => p.Recipient)
                .Column("[RecipientID]");

            References(p => p.Sender)
                .Column("[SenderID]");

            DynamicInsert();
        }
    }
}