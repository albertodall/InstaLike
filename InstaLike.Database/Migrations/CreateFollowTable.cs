using FluentMigrator;
using FluentMigrator.SqlServer;

namespace Instalike.Database.Migrations
{
    [Migration(2, "Create 'Follow' table in schema 'dbo'")]
    public class CreateFollowTable : AutoReversingMigration
    {
        public override void Up()
        {
            Create.Table("Follow").InSchema("dbo")
                .WithColumn("ID").AsInt32().Identity(1, 1);

            Create.PrimaryKey("PK_Follow").OnTable("Follow").WithSchema("dbo").Column("ID").Clustered();

            Create.Column("FollowerID").OnTable("Follow").InSchema("dbo").AsInt32().NotNullable();
            Create.Column("FollowedID").OnTable("Follow").InSchema("dbo").AsInt32().NotNullable();
            Create.Column("FollowDate").OnTable("Follow").InSchema("dbo").AsDateTimeOffset().NotNullable().WithDefault(SystemMethods.CurrentDateTimeOffset);

            Create.Index("IX_Follow_Followed")
                .OnTable("Follow").InSchema("dbo")
                .OnColumn("FollowedID")
                .Ascending().WithOptions().NonClustered();

            Create.Index("IX_Follow_Follower")
                .OnTable("Follow").InSchema("dbo")
                .OnColumn("FollowerID")
                .Ascending().WithOptions().NonClustered();

            Create.UniqueConstraint("IX_Follow_Follower_Followed")
                .OnTable("Follow").WithSchema("dbo")
                .Columns(new[] { "FollowerID", "FollowedID" })
                .NonClustered();

            Create.ForeignKey("FK_User_Follower")
                .FromTable("Follow").InSchema("dbo").ForeignColumn("FollowerID")
                .ToTable("User").InSchema("dbo").PrimaryColumn("ID");

            Create.ForeignKey("FK_User_Followed")
                .FromTable("Follow").InSchema("dbo").ForeignColumn("FollowedID")
                .ToTable("User").InSchema("dbo").PrimaryColumn("ID");
        }
    }
}
