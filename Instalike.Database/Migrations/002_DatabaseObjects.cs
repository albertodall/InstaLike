using FluentMigrator;

namespace Instalike.Database.Migrations
{
    public class DatabaseObjects : Migration
    {
        public override void Up()
        {
            Execute.EmbeddedScript(@"Scripts\UserTable.sql");
            Execute.EmbeddedScript(@"Scripts\FollowTable.sql");
            Execute.EmbeddedScript(@"Scripts\NotificationTable.sql");
            Execute.EmbeddedScript(@"Scripts\PostTable.sql");
            Execute.EmbeddedScript(@"Scripts\CommentTable.sql");
            Execute.EmbeddedScript(@"Scripts\LikeTable.sql");
        }

        public override void Down()
        {
            Execute.Sql("DROP TABLE [dbo].[Like]");
            Execute.Sql("DROP TABLE [dbo].[Comment]");
            Execute.Sql("DROP TABLE [dbo].[Post]");
            Execute.Sql("DROP TABLE [dbo].[Notification]");
            Execute.Sql("DROP TABLE [dbo].[Follow]");
            Execute.Sql("DROP TABLE [dbo].[User]");
        }
    }
}
