using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using InstaLike.Web.Services;
using NHibernate.Connection;
using NHibernate.Driver;

namespace InstaLike.Web.Infrastructure
{
    /// <summary>
    /// Strategy to get a connection to both the database and the external storage (Azure Blob Storage) holding pictures.
    /// </summary>
    internal class HybridStorageDriverConnectionProvider : IConnectionProvider
    {
        private readonly IConnectionProvider _connectionProvider;
        private IHybridStorageConnectionProvider? _externalStorageConnectionProvider;

        public HybridStorageDriverConnectionProvider()
        {
            _connectionProvider = new DriverConnectionProvider();
        }

        public IDriver Driver => new HybridStorageDriver(_connectionProvider.Driver);

        public void CloseConnection(DbConnection conn)
        {
            _connectionProvider.CloseConnection(conn);
        }

        public void Configure(IDictionary<string, string> settings)
        {
            _connectionProvider.Configure(settings);

            if (!settings.TryGetValue(ExternalStorageParameters.ConnectionProviderProperty, out string? type) || string.IsNullOrEmpty(type))
            {
                throw new InvalidOperationException($"External storage provider has not been configured. Set the '{ExternalStorageParameters.ConnectionProviderProperty}' property.");
            }

            var externalStorageProviderType = Type.GetType(type) ?? throw new ArgumentException($"Unknown storage provider type: {type}", nameof(settings));
            _externalStorageConnectionProvider = (IHybridStorageConnectionProvider?)Activator.CreateInstance(externalStorageProviderType)
                ?? throw new InvalidOperationException($"Unable to create storage provider of type {type}");
            _externalStorageConnectionProvider.Configure(settings);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _connectionProvider.Dispose();
            }
        }

        public DbConnection GetConnection()
        {
            return new HybridStorageConnection(_connectionProvider.GetConnection(), GetExternalStorageProvider());
        }

        public async Task<DbConnection> GetConnectionAsync(CancellationToken cancellationToken)
        {
            return new HybridStorageConnection(await _connectionProvider.GetConnectionAsync(cancellationToken), GetExternalStorageProvider());
        }

        private IExternalStorageProvider GetExternalStorageProvider()
        {
            return _externalStorageConnectionProvider?.GetProvider()
                ?? throw new InvalidOperationException("External storage provider has not been configured.");
        }
    }
}