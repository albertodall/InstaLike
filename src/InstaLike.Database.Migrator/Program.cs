using System.Reflection;
using DbUp;

const string ConnectionStringVariable = "ConnectionStrings__InstalikeDatabase";

var connectionString = args.FirstOrDefault() ?? Environment.GetEnvironmentVariable(ConnectionStringVariable);
if (string.IsNullOrWhiteSpace(connectionString))
{
    Console.Error.WriteLine($"Usage: InstaLike.Database.Migrator <connection string> (or set the {ConnectionStringVariable} environment variable).");
    return 2;
}

// Creates the database when missing (local and containerized environments); Azure SQL databases are provisioned by Terraform.
EnsureDatabase.For.SqlDatabase(connectionString);

var upgrader = DeployChanges.To
    .SqlDatabase(connectionString)
    .WithScriptsEmbeddedInAssembly(Assembly.GetExecutingAssembly())
    .WithTransactionPerScript()
    .LogToConsole()
    .Build();

var result = upgrader.PerformUpgrade();
if (!result.Successful)
{
    Console.Error.WriteLine(result.Error);
    return 1;
}

Console.WriteLine("Database is up to date.");
return 0;
