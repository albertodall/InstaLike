using FluentMigrator;
using FluentMigrator.SqlServer;

namespace Instalike.Database.Migrations
{
    [Migration(6, "Create 'Notification' table in schema 'dbo'")]
    public class CreateNotificationTable : AutoReversingMigration
    {
        public override void Up()
        {
            Create.Table("Notification").InSchema("dbo")
               .WithColumn("ID").AsInt32().Identity(1, 1);

            Create.Column("SenderID").OnTable("Notification").InSchema("dbo").AsInt32().NotNullable();
            Create.Column("RecipientID").OnTable("Notification").InSchema("dbo").AsInt32().NotNullable();
            Create.Column("Message").OnTable("Notification").InSchema("dbo").AsString(200).NotNullable().WithDefaultValue(string.Empty);
            Create.Column("ReadByRecipient").OnTable("Notification").InSchema("dbo").AsBoolean().NotNullable().WithDefaultValue(false);
            Create.Column("NotificationDate").OnTable("Notification").InSchema("dbo").AsDateTimeOffset().NotNullable().WithDefault(SystemMethods.CurrentDateTimeOffset);

            Create.PrimaryKey("PK_Notification").OnTable("Notification").WithSchema("dbo").Column("ID").Clustered();

            Create.Index("IX_Notification_Recipient")
                .OnTable("Notification").InSchema("dbo")
                .WithOptions().NonClustered()
                .OnColumn("RecipientID")
                    .Ascending()
                    .Include("ReadByRecipient");
        }
    }
}
