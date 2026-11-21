using InstaLike.Database.Migrator;

const string ConnectionStringVariable = "ConnectionStrings__InstalikeDatabase";
const string ApplicationUserNameVariable = "ApplicationUser__Name";
const string ApplicationUserClientIdVariable = "ApplicationUser__ClientId";

var connectionString = args.FirstOrDefault() ?? Environment.GetEnvironmentVariable(ConnectionStringVariable);
if (string.IsNullOrWhiteSpace(connectionString))
{
    Console.Error.WriteLine($"Usage: InstaLike.Database.Migrator <connection string> (or set the {ConnectionStringVariable} environment variable).");
    return 2;
}

// In Azure, grants database access to the web app's managed identity.
var applicationUserName = Environment.GetEnvironmentVariable(ApplicationUserNameVariable);
var applicationUserClientId = Environment.GetEnvironmentVariable(ApplicationUserClientIdVariable);
var result = !string.IsNullOrWhiteSpace(applicationUserName) && Guid.TryParse(applicationUserClientId, out var clientId)
    ? DatabaseMigration.UpgradeGrantingAccessTo(connectionString, applicationUserName, clientId)
    : DatabaseMigration.Upgrade(connectionString);

if (!result.Successful)
{
    Console.Error.WriteLine(result.Error);
    return 1;
}

Console.WriteLine("Database is up to date.");
return 0;
