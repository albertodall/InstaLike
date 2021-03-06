﻿using FluentMigrator;

namespace InstaLike.Database.Migrations
{
    [Tags("SqlServerOnPrem")]
    [Migration(7, "Add profile picture support for 'User' table")]
    public class UserAddProfilePicture : Migration
    {
        public override void Up()
        {
            Execute.Script(@"Scripts\UserAddProfilePicture_OnPrem.sql");
        }

        public override void Down()
        {
            Execute.Script(@"Scripts\UserRemoveProfilePicture_OnPrem.sql");
        }
    }

    [Tags("SqlAzure")]
    [Migration(7, "Add profile picture support for 'User' table")]
    public class UserAddProfilePictureAzureSql : Migration
    {
        public override void Up()
        {
            Execute.Script(@"Scripts\UserAddProfilePicture_SqlAzure.sql");
        }

        public override void Down()
        {
            Execute.Script(@"Scripts\UserRemoveProfilePicture_SqlAzure.sql");
        }
    }
}
