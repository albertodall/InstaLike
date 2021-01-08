using FluentMigrator;

namespace InstaLike.Database.Migrations
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

    [Tags("SqlAzure")]
    [Migration(8, "Add picture support to 'Post' table")]
    public class PostAddPictureSqlAzure : Migration
    {
        public override void Up()
        {
            Execute.Script(@"Scripts\PostAddPicture_SqlAzure.sql");
        }

        public override void Down()
        {
            Execute.Script(@"Scripts\PostRemovePicture_SqlAzure.sql");
        }
    }
}
