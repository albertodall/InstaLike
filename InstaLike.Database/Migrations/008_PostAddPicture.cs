using FluentMigrator;

namespace Instalike.Database.Migrations
{
    [Tags("SqlServerOnPrem")]
    [Migration(8, "Add picture support to 'Post' table")]
    public class PostAddPicture : Migration
    {
        public override void Up()
        {
            Execute.Script(@"Scripts\PostAddPicture_OnPrem.sql");
        }

        public override void Down()
        {
            Execute.Script(@"Scripts\PostRemovePicture_OnPrem.sql");
        }
    }
}
