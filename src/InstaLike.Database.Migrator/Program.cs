using System.Reflection;
using DbUp;
using DbUp.Engine;
using DbUp.Support;

const string ConnectionStringVariable = "ConnectionStrings__InstalikeDatabase";
const string ApplicationUserNameVariable = "ApplicationUser__Name";
const string ApplicationUserClientIdVariable = "ApplicationUser__ClientId";
const string GrantsFolder = ".mssql.grants.";

var connectionString = args.FirstOrDefault() ?? Environment.GetEnvironmentVariable(ConnectionStringVariable);
if (string.IsNullOrWhiteSpace(connectionString))
{
    Console.Error.WriteLine($"Usage: InstaLike.Database.Migrator <connection string> (or set the {ConnectionStringVariable} environment variable).");
    return 2;
}

// Creates the database when missing (local and containerized environments); Azure SQL databases are provisioned by Terraform.
EnsureDatabase.For.SqlDatabase(connectionString);

var assembly = Assembly.GetExecutingAssembly();
var builder = DeployChanges.To
    .SqlDatabase(connectionString)
    .WithScriptsEmbeddedInAssembly(assembly, script => !script.Contains(GrantsFolder))
    .WithTransactionPerScript()
    .LogToConsole();

// In Azure, grants database access to the web app's managed identity.
var applicationUserName = Environment.GetEnvironmentVariable(ApplicationUserNameVariable);
var applicationUserClientId = Environment.GetEnvironmentVariable(ApplicationUserClientIdVariable);
if (!string.IsNullOrWhiteSpace(applicationUserName) && Guid.TryParse(applicationUserClientId, out var clientId))
{
    builder
        .WithScriptsEmbeddedInAssembly(
            assembly,
            script => script.Contains(GrantsFolder),
            new SqlScriptOptions { ScriptType = ScriptType.RunAlways, RunGroupOrder = DbUpDefaults.DefaultRunGroupOrder + 1 })
        .WithVariable("ApplicationUserName", applicationUserName)
        .WithVariable("ApplicationUserClientId", clientId.ToString());
}

var result = builder.Build().PerformUpgrade();
if (!result.Successful)
{
    Console.Error.WriteLine(result.Error);
    return 1;
}

Console.WriteLine("Database is up to date.");
return 0;
