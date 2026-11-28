using FluentNHibernate.Mapping;
using InstaLike.Core.Notifications;
using InstaLike.Core.SharedKernel;
using InstaLike.Web.Data.Types;

namespace InstaLike.Web.Data.Mapping
{
    internal sealed class NotificationMapping : ClassMap<Notification>
    {
        public NotificationMapping()
        {
            Table("[Notification]");

            Id(p => p.ID).CustomType<TypedIdType<NotificationId>>().GeneratedBy.Assigned();

            Map(p => p.HasBeenReadByRecipient)
                .Column("ReadByRecipient")
                .Not.Nullable();

            Map(p => p.Kind).CustomType<NotificationKind>()
                .Not.Nullable();

            Map(p => p.NotificationDate)
                .Not.Nullable();

            Map(p => p.RecipientID)
                .CustomType<TypedIdType<MemberId>>()
                .Not.Nullable();

            Map(p => p.SenderID)
                .CustomType<TypedIdType<MemberId>>()
                .Not.Nullable();

            Map(p => p.PostID)
                .CustomType<TypedIdType<PostId>>()
                .Nullable();
        }
    }
}
