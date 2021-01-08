using FluentMigrator;
using FluentMigrator.SqlServer;

namespace InstaLike.Database.Migrations
{
    [Migration(4, "Create 'Comment' table in schema 'dbo'")]
    public class CreateCommentTable : AutoReversingMigration
    {
        public override void Up()
        {
            Create.Table("Comment").InSchema("dbo")
                .WithColumn("ID").AsInt32().Identity(1, 1);

            Create.Column("Text")
                .OnTable("Comment").InSchema("dbo")
                .AsAnsiString(500)
                .NotNullable().WithDefaultValue(string.Empty);
            Create.Column("UserID")
                .OnTable("Comment").InSchema("dbo")
                .AsInt32()
                .NotNullable();
            Create.Column("PostID")
                .OnTable("Comment").InSchema("dbo")
                .AsInt32()
                .NotNullable();
            Create.Column("CommentDate")
                .OnTable("Comment").InSchema("dbo")
                .AsDateTimeOffset()
                .NotNullable().WithDefault(SystemMethods.CurrentDateTimeOffset);

            Create.PrimaryKey("PK_Comment")
                .OnTable("Comment").WithSchema("dbo")
                .Column("ID").Clustered();

            Create.Index("IX_Comment_Post")
                .OnTable("Comment").InSchema("dbo").OnColumn("PostID")
                .Ascending()
                .WithOptions().NonClustered();

            Create.ForeignKey("FK_Post_Comment")
                .FromTable("Comment").InSchema("dbo").ForeignColumn("PostID")
                .ToTable("Post").InSchema("dbo").PrimaryColumn("ID");

            Create.ForeignKey("FK_User_Comment")
                .FromTable("Comment").InSchema("dbo").ForeignColumn("UserID")
                .ToTable("User").InSchema("dbo").PrimaryColumn("ID");
        }
    }
}