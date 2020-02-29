using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using NHibernate.Connection;
using NHibernate.Driver;

namespace InstaLike.Web.Infrastructure
{
    /// <summary>
    /// Strategy to get a connection to both the database and the external storage.
    /// </summary>
    internal class HybridStorageDriverConnectionProvider : IConnectionProvider
    {
        private readonly IConnectionProvider _connectionProvider;
        private IHybridStorageConnectionProvider _externalStorageConnectionProvider;

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

            // Configure external storage, if specified
            if (settings.TryGetValue(ExternalStorageParameters.ConnectionProviderProperty, out string type) && type != null)
            {
                var externalStorageProviderType = Type.GetType(type);
                _externalStorageConnectionProvider = (IHybridStorageConnectionProvider)Activator.CreateInstance(externalStorageProviderType);
                _externalStorageConnectionProvider.Configure(settings);
            }
        }

        public void Dispose()
        {
            _connectionProvider.Dispose();
        }

        public DbConnection GetConnection()
        {
            var connection = _connectionProvider.GetConnection();
            if (_externalStorageConnectionProvider == null)
            {
                return connection;
            }

            return new HybridStorageConnection(connection, _externalStorageConnectionProvider.GetProvider());
        }

        public async Task<DbConnection> GetConnectionAsync(CancellationToken cancellationToken)
        {
            var connection = await _connectionProvider.GetConnectionAsync(cancellationToken);
            if (_externalStorageConnectionProvider == null)
            {
                return connection;
            }

            return new HybridStorageConnection(connection, _externalStorageConnectionProvider.GetProvider());
        }
    }
}