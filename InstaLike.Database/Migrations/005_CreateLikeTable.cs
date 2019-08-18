using FluentMigrator;
using FluentMigrator.SqlServer;

namespace Instalike.Database.Migrations
{
    [Migration(5, "Create 'Like' table in schema 'dbo'")]
    public class CreateLikeTable : AutoReversingMigration
    {
        public override void Up()
        {
            Create.Table("Like").InSchema("dbo")
               .WithColumn("ID").AsInt32().Identity(1, 1);

            Create.Column("PostID")
                .OnTable("Like").InSchema("dbo")
                .AsInt32()
                .NotNullable();
            Create.Column("UserID")
                .OnTable("Like").InSchema("dbo")
                .AsInt32()
                .NotNullable();
            Create.Column("LikeDate")
                .OnTable("Like").InSchema("dbo")
                .AsDateTimeOffset()
                .NotNullable().WithDefault(SystemMethods.CurrentDateTimeOffset);

            Create.PrimaryKey("PK_Like")
                .OnTable("Like").WithSchema("dbo")
                .Column("ID").Clustered();

            Create.ForeignKey("FK_Post_Like")
                .FromTable("Like").InSchema("dbo").ForeignColumn("PostID")
                .ToTable("Post").InSchema("dbo").PrimaryColumn("ID");

            Create.ForeignKey("FK_User_Like")
                .FromTable("Like").InSchema("dbo").ForeignColumn("UserID")
                .ToTable("User").InSchema("dbo").PrimaryColumn("ID");

            Create.Index("IX_Like_PostUser")
                .OnTable("Like").InSchema("dbo")
                .OnColumn("PostID").Ascending()
                .OnColumn("UserID").Ascending()
                .WithOptions().NonClustered();
        }
    }
}
