using FluentMigrator;
using FluentMigrator.SqlServer;

namespace InstaLike.Database.Migrations
{
    [Migration(1, "Create 'User' Table in schema 'dbo'")]
    public class CreateUserTable : AutoReversingMigration
    {
        public override void Up()
        {
            Create.Table("User").InSchema("dbo")
                .WithColumn("ID").AsInt32().Identity(1, 1);

            Create.Column("Nickname")
                .OnTable("User").InSchema("dbo")
                .AsAnsiString(20)
                .NotNullable().WithDefaultValue(string.Empty);
            Create.Column("Password")
                .OnTable("User").InSchema("dbo")
                .AsAnsiString(64)
                .NotNullable().WithDefaultValue(string.Empty);
            Create.Column("Name")
                .OnTable("User").InSchema("dbo")
                .AsAnsiString(30)
                .NotNullable().WithDefaultValue(string.Empty);
            Create.Column("Surname")
                .OnTable("User").InSchema("dbo")
                .AsAnsiString(50)
                .NotNullable().WithDefaultValue(string.Empty);
            Create.Column("Email")
                .OnTable("User").InSchema("dbo")
                .AsAnsiString(50)
                .NotNullable().WithDefaultValue(string.Empty);
            Create.Column("Biography")
                .OnTable("User").InSchema("dbo")
                .AsAnsiString(500)
                .NotNullable().WithDefaultValue(string.Empty);
            Create.Column("RegistrationDate")
                .OnTable("User").InSchema("dbo")
                .AsDateTimeOffset()
                .NotNullable().WithDefault(SystemMethods.CurrentDateTimeOffset);

            Create.PrimaryKey("PK_User")
                .OnTable("User").WithSchema("dbo")
                .Column("ID").Clustered();

            Create.Index("IX_User_Credentials").OnTable("User").InSchema("dbo")
                .OnColumn("Nickname").Ascending()
                .OnColumn("Password").Ascending()
                .WithOptions().NonClustered();

            Create.UniqueConstraint("IX_User_Nickname")
                .OnTable("User").WithSchema("dbo")
                .Column("Nickname").NonClustered();
        }
    }
}
