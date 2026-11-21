using System;
using System.IO;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using InstaLike.Core.SharedKernel;
using InstaLike.Database.Migrator;
using InstaLike.IntegrationTests.Properties;
using InstaLike.Web.Infrastructure;
using InstaLike.Web.Media;
using Microsoft.Data.SqlClient;
using NHibernate;
using Respawn;
using Respawn.Graph;
using Serilog;
using Testcontainers.Azurite;
using Testcontainers.MsSql;
using Xunit;

namespace InstaLike.IntegrationTests
{
    /// <summary>
    /// Starts SQL Server, migrated by the migrator's scripts, and Azurite once for the whole test run,
    /// and empties both before each test.
    /// </summary>
    [CollectionDefinition(CollectionName)]
    public sealed class TestInfrastructureFactory : IAsyncLifetime, ICollectionFixture<TestInfrastructureFactory>
    {
        public const string CollectionName = "Infrastructure";

        private const string SqlServerImage = "mcr.microsoft.com/mssql/server:2022-latest";
        private const string AzuriteImage = "mcr.microsoft.com/azure-storage/azurite:latest";
        private const string DatabaseName = "InstaLike";

        private readonly MsSqlContainer _databaseContainer = new MsSqlBuilder(SqlServerImage).Build();
        private readonly AzuriteContainer _storageContainer = new AzuriteBuilder(AzuriteImage)
            .WithCommand("--skipApiVersionCheck")
            .Build();

        private string _connectionString = string.Empty;
        private Respawner? _respawner;
        private BlobServiceClient? _blobServiceClient;
        private ISessionFactory? _sessionFactory;
        private IPictureStore? _pictureStore;

        public ISessionFactory SessionFactory => _sessionFactory ?? throw new InvalidOperationException("The test database has not been started.");

        public IPictureStore PictureStore => _pictureStore ?? throw new InvalidOperationException("The test storage has not been started.");

        public async ValueTask InitializeAsync()
        {
            await Task.WhenAll(_databaseContainer.StartAsync(), _storageContainer.StartAsync());

            _connectionString = new SqlConnectionStringBuilder(_databaseContainer.GetConnectionString()) { InitialCatalog = DatabaseName }.ConnectionString;

            var migration = DatabaseMigration.Upgrade(_connectionString);
            if (!migration.Successful)
            {
                throw new InvalidOperationException("The test database could not be migrated.", migration.Error);
            }

            _sessionFactory = ServiceCollectionExtensions.GetFluentConfigurationForDatabase(_connectionString).BuildSessionFactory();
            _respawner = await CreateRespawnerAsync();

            _blobServiceClient = new BlobServiceClient(_storageContainer.GetConnectionString());
            await AzureBlobPictureStore.EnsureContainersExistAsync(_blobServiceClient);
            _pictureStore = new AzureBlobPictureStore(_blobServiceClient, Log.Logger);
        }

        /// <summary>
        /// Empties the database, except the migration journal, and the picture containers.
        /// </summary>
        public async Task ResetAsync()
        {
            if (_respawner is null || _blobServiceClient is null)
            {
                throw new InvalidOperationException("The test infrastructure has not been started.");
            }

            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            await _respawner.ResetAsync(connection);

            foreach (var containerName in PictureContainers.All)
            {
                var container = _blobServiceClient.GetBlobContainerClient(containerName);
                await foreach (var blob in container.GetBlobsAsync(new GetBlobsOptions(), TestContext.Current.CancellationToken))
                {
                    await container.DeleteBlobAsync(blob.Name, cancellationToken: TestContext.Current.CancellationToken);
                }
            }
        }

        /// <summary>
        /// A session that writes its SQL statements to the test output.
        /// </summary>
        public ISession OpenSession()
        {
            return SessionFactory
                .WithOptions()
                    .Interceptor(new XUnitSqlStatementOutputInterceptor())
                .OpenSession();
        }

        /// <summary>
        /// A stateless session, like the ones the application's queries use.
        /// </summary>
        public IStatelessSession OpenStatelessSession()
        {
            return SessionFactory.OpenStatelessSession();
        }

        /// <summary>
        /// Reads a stored picture back, or returns null when the store has none with that ID.
        /// </summary>
        public async Task<byte[]?> ReadPictureAsync(string container, PictureId picture)
        {
            await using var stored = await PictureStore.OpenReadAsync(container, picture, TestContext.Current.CancellationToken);
            if (stored is null)
            {
                return null;
            }

            using var content = new MemoryStream();
            await stored.CopyToAsync(content, TestContext.Current.CancellationToken);

            return content.ToArray();
        }

        /// <summary>
        /// The test picture embedded in the test assembly, encoded in Base64.
        /// </summary>
        public static string GetTestPictureBase64()
        {
            return Convert.ToBase64String(Resources.GrumpyCat);
        }

        public async ValueTask DisposeAsync()
        {
            _sessionFactory?.Dispose();
            await _storageContainer.DisposeAsync();
            await _databaseContainer.DisposeAsync();
        }

        /// <summary>
        /// Prepares Respawn to empty every table except the migration journal.
        /// </summary>
        private async Task<Respawner> CreateRespawnerAsync()
        {
            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            return await Respawner.CreateAsync(
                connection,
                new RespawnerOptions
                {
                    TablesToIgnore =
                    [
                        new Table(DatabaseMigration.JournalSchema, DatabaseMigration.JournalTable)
                    ]
                });
        }
    }
}
