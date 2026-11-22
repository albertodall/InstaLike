using System;
using System.Threading.Tasks;
using InstaLike.Database.Migrator;
using InstaLike.Web.Media;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Respawn;
using Respawn.Graph;
using Testcontainers.Azurite;
using Testcontainers.MsSql;
using Xunit;

namespace InstaLike.Web.EndToEndTests
{
    [CollectionDefinition(CollectionName)]
    public sealed class InstaLikeApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime, ICollectionFixture<InstaLikeApplicationFactory>
    {
        public const string CollectionName = "Application";

        private const string SqlServerImage = "mcr.microsoft.com/mssql/server:2022-latest";
        private const string AzuriteImage = "mcr.microsoft.com/azure-storage/azurite:latest";
        private const string DatabaseName = "InstaLike";

        private readonly MsSqlContainer _databaseContainer = new MsSqlBuilder(SqlServerImage).Build();
        private readonly AzuriteContainer _storageContainer = new AzuriteBuilder(AzuriteImage)
            .WithCommand("--skipApiVersionCheck")
            .Build();

        private string _databaseConnectionString = string.Empty;
        private Respawner? _respawner;

        public async ValueTask InitializeAsync()
        {
            await Task.WhenAll(_databaseContainer.StartAsync(), _storageContainer.StartAsync());

            _databaseConnectionString = new SqlConnectionStringBuilder(_databaseContainer.GetConnectionString()) { InitialCatalog = DatabaseName }.ConnectionString;

            var migration = DatabaseMigration.Upgrade(_databaseConnectionString);
            if (!migration.Successful)
            {
                throw new InvalidOperationException("The test database could not be migrated.", migration.Error);
            }

            await using var connection = new SqlConnection(_databaseConnectionString);
            await connection.OpenAsync();
            _respawner = await Respawner.CreateAsync(
                connection,
                new RespawnerOptions
                {
                    TablesToIgnore = [new Table(DatabaseMigration.JournalSchema, DatabaseMigration.JournalTable)]
                });
        }

        /// <summary>
        /// Empties every table except the migration journal.
        /// </summary>
        public async Task ResetDatabaseAsync()
        {
            if (_respawner is null)
            {
                throw new InvalidOperationException("The test database has not been started.");
            }

            await using var connection = new SqlConnection(_databaseConnectionString);
            await connection.OpenAsync();
            await _respawner.ResetAsync(connection);
        }

        /// <summary>
        /// A new browser with its own cookies, which browses over HTTPS and does not follow redirects.
        /// </summary>
        public Browser OpenBrowser()
        {
            return new Browser(
                CreateClient(new WebApplicationFactoryClientOptions
                {
                    BaseAddress = new Uri("https://localhost/"),
                    AllowAutoRedirect = false
                })
            );
        }

        /// <summary>
        /// A new browser with its own cookies, which browses over HTTP and does not follow redirects.
        /// </summary>
        public Browser OpenBrowserOverHttp()
        {
            return new Browser(
                CreateClient(new WebApplicationFactoryClientOptions
                {
                    BaseAddress = new Uri("http://localhost/"),
                    AllowAutoRedirect = false
                })
            );
        }

        public override async ValueTask DisposeAsync()
        {
            await base.DisposeAsync();
            await _storageContainer.DisposeAsync();
            await _databaseContainer.DisposeAsync();
        }

        /// <summary>
        /// Runs the application in production mode on the test containers, with a fake image recognition.
        /// </summary>
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment(Environments.Production);
            builder.UseSetting("ConnectionStrings:InstalikeDatabase", _databaseConnectionString);
            builder.UseSetting("ExternalStorage:AzureBlobStorage:ConnectionString", _storageContainer.GetConnectionString());
            builder.UseSetting("ImageAnalysis:Endpoint", "http://localhost/");
            builder.UseSetting("ImageAnalysis:Model", "fake");
            builder.ConfigureTestServices(services => services.AddSingleton<IImageRecognitionService, FakeImageRecognitionService>());
        }
    }
}
