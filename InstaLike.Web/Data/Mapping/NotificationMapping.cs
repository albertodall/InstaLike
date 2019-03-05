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

            Map(p => p.IsRead).CustomType<bool>()
                .Column("Read")
                .Not.Nullable();

            Map(p => p.Message).CustomType<string>()
                .Not.Nullable();

            Map(p => p.NotificationDate).CustomType<DateTimeOffset>()
                .Not.Nullable();

            References(p => p.Recipient)
                .Column("RecipientID")
                .Not.Nullable();

            References(p => p.Sender)
                .Column("SenderID")
                .Not.Nullable();

            DynamicInsert();
        }
    }
}
