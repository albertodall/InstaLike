using FluentNHibernate.Mapping;
using InstaLike.Core.Notifications;

namespace InstaLike.Web.Data.Mapping;

internal class NotificationMapping : ClassMap<Notification>
{
    public NotificationMapping()
    {
        Table("[Notification]");

        Id(p => p.ID).GeneratedBy.Native();

        Map(p => p.HasBeenReadByRecipient).CustomType<bool>()
            .Column("[ReadByRecipient]")
            .Not.Nullable();

        Map(p => p.Kind).CustomType<NotificationKind>()
            .Not.Nullable();

        Map(p => p.NotificationDate)
            .Not.Nullable();

        Map(p => p.RecipientID)
            .Column("[RecipientID]")
            .Not.Nullable();

        Map(p => p.SenderID)
            .Column("[SenderID]")
            .Not.Nullable();

        Map(p => p.PostID)
            .Nullable();

        DynamicInsert();
    }
}
