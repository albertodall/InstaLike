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
    internal class HybridStorageConnection : DbConnection, IExternalStorageProvider
    {
        private readonly IExternalStorageProvider _externalStorage;

        public HybridStorageConnection(DbConnection database, IExternalStorageProvider externalStorage)
        {
            DatabaseConnection = database ?? throw new ArgumentNullException(nameof(database));
            _externalStorage = externalStorage ?? throw new ArgumentNullException(nameof(externalStorage));
        }

        public override string ConnectionString
        {
            get => DatabaseConnection.ConnectionString;
            set => DatabaseConnection.ConnectionString = value;
        }

        public override string Database => DatabaseConnection.Database;

        public override string DataSource => null;

        public override string ServerVersion => null;

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

        public async Task<Picture> LoadPictureAsync(string blobFileName, string containerName)
        {
            return await _externalStorage.LoadPictureAsync(blobFileName, containerName);
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

        internal DbConnection DatabaseConnection { get; }
    }
}