using System;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;
using InstaLike.Core.Domain;
using InstaLike.Web.Services;

namespace InstaLike.Web.Infrastructure
{
    /// <summary>
    /// This connection "connects" the O/RM to both the database and the external storage.
    /// </summary>
    internal class HybridStorageConnection : DbConnection
    {
        private readonly IExternalStorageProvider _externalStorage;

        public HybridStorageConnection(DbConnection databaseConnection, IExternalStorageProvider externalStorage)
        {
            DatabaseConnection = databaseConnection ?? throw new ArgumentNullException(nameof(databaseConnection));
            _externalStorage = externalStorage ?? throw new ArgumentNullException(nameof(externalStorage));
        }

#pragma warning disable CS8765 // Nullability of type of parameter doesn't match overridden member (possibly because of nullability attributes).
        public override string ConnectionString
        {
            get => DatabaseConnection.ConnectionString;
            set => DatabaseConnection.ConnectionString = value;
        }
#pragma warning restore CS8765

        public override string Database => DatabaseConnection.Database;

        public override string DataSource => string.Empty;

        public override string ServerVersion => string.Empty;

        public override ConnectionState State => DatabaseConnection.State;

        public override void ChangeDatabase(string databaseName)
        {
            DatabaseConnection.ChangeDatabase(databaseName);
        }

        public override void Close()
        {
            DatabaseConnection.Close();
        }

        public override void Open()
        {
            DatabaseConnection.Open();
        }

        protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel)
        {
            return DatabaseConnection.BeginTransaction(isolationLevel);
        }

        protected override DbCommand CreateDbCommand()
        {
            return new HybridStorageCommand(this, DatabaseConnection.CreateCommand());
        }

        public Task<Picture> LoadPictureAsync(string blobFileName, string containerName)
        {
            return _externalStorage.LoadPictureAsync(blobFileName, containerName);
        }

        public async Task SavePictureAsync(Picture picture, string containerName)
        {
            await _externalStorage.SavePictureAsync(picture, containerName);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                DatabaseConnection.Dispose();
            }
            base.Dispose(disposing);
        }

        public DbConnection DatabaseConnection { get; }
    }
}