using FluentMigrator;
using FluentMigrator.SqlServer;

namespace InstaLike.Database.Migrations
{
    [Migration(3, "Create 'Post' table in schema 'dbo'")]
    public class CreatePostTable : AutoReversingMigration
    {
        public override void Up()
        {
            Create.Table("Post").InSchema("dbo")
                .WithColumn("ID").AsInt32().Identity(1, 1);

            Create.Column("UserID")
                .OnTable("Post").InSchema("dbo")
                .AsInt32()
                .NotNullable();
            Create.Column("Text")
                .OnTable("Post").InSchema("dbo")
                .AsAnsiString(500)
                .NotNullable().WithDefaultValue(string.Empty);
            Create.Column("PostDate")
                .OnTable("Post").InSchema("dbo")
                .AsDateTimeOffset()
                .NotNullable().WithDefault(SystemMethods.CurrentDateTimeOffset);

            Create.PrimaryKey("PK_Post")
                .OnTable("Post").WithSchema("dbo")
                .Column("ID").Clustered();

            Create.ForeignKey("FK_User_Post")
                .FromTable("Post").InSchema("dbo").ForeignColumn("UserID")
                .ToTable("User").InSchema("dbo").PrimaryColumn("ID");
        }
    }
}
