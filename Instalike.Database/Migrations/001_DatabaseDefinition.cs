using System;
using FluentMigrator;

namespace Instalike.Database.Migrations
{
    public class DatabaseDefinition : Migration
    {
        public override void Up()
        {
            Execute.Script(@"Scripts\DatabaseDefinition.sql");
        }

        public override void Down()
        {
            Execute.Sql("DROP DATABASE InstaLike");
        }
    }
}
