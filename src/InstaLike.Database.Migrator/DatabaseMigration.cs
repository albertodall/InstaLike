using System.Reflection;
using DbUp;
using DbUp.Builder;
using DbUp.Engine;
using DbUp.Support;

namespace InstaLike.Database.Migrator
{
    public static class DatabaseMigration
    {
        public const string JournalSchema = "System";
        public const string JournalTable = "Migrations";

        private const string GrantsFolder = ".mssql.grants.";

        public static DatabaseUpgradeResult Upgrade(string connectionString)
        {
            return CreateBuilder(connectionString).Build().PerformUpgrade();
        }

        public static DatabaseUpgradeResult UpgradeGrantingAccessTo(string connectionString, string applicationUserName, Guid applicationUserClientId)
        {
            return CreateBuilder(connectionString)
                .WithScriptsEmbeddedInAssembly(
                    Assembly.GetExecutingAssembly(),
                    script => script.Contains(GrantsFolder),
                    new SqlScriptOptions
                    {
                        ScriptType = ScriptType.RunAlways,
                        RunGroupOrder = DbUpDefaults.DefaultRunGroupOrder + 1
                    })
                .WithVariable("ApplicationUserName", applicationUserName)
                .WithVariable("ApplicationUserClientId", applicationUserClientId.ToString())
                .Build()
                .PerformUpgrade();
        }

        private static UpgradeEngineBuilder CreateBuilder(string connectionString)
        {
            // Creates the database when missing (local and containerized environments); Azure SQL databases are provisioned by Terraform.
            EnsureDatabase.For.SqlDatabase(connectionString);

            return DeployChanges.To
                .SqlDatabase(connectionString, JournalSchema)
                .WithScriptsEmbeddedInAssembly(Assembly.GetExecutingAssembly(), script => !script.Contains(GrantsFolder))
                .JournalToSqlTable(JournalSchema, JournalTable)
                .WithTransactionPerScript()
                .LogToConsole();
        }
    }
}
