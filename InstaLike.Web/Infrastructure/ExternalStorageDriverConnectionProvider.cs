using System;
using System.Collections.Generic;
using System.Configuration;
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
    internal class ExternalStorageDriverConnectionProvider : IConnectionProvider
    {
        private readonly IConnectionProvider _connectionProvider;
        private IExternalStorageConnectionProvider _externalStorageConnectionProvider;

        public ExternalStorageDriverConnectionProvider()
        {
            _connectionProvider = new DriverConnectionProvider();
        }

        public IDriver Driver => new ExternalStorageDriver(_connectionProvider.Driver);

        public void CloseConnection(DbConnection conn)
        {
            _connectionProvider.CloseConnection(conn);
        }

        public void Configure(IDictionary<string, string> settings)
        {
            _externalStorageConnectionProvider = new AzureBlobStorageConnectionProvider("UseDevelopmentStorage=true");
            _connectionProvider.Configure(settings);
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

            return new ExternalStorageDatabaseConnection(connection, _externalStorageConnectionProvider.GetProvider());
        }

        public async Task<DbConnection> GetConnectionAsync(CancellationToken cancellationToken)
        {
            var connection = await _connectionProvider.GetConnectionAsync(cancellationToken);
            if (_externalStorageConnectionProvider == null)
            {
                return connection;
            }

            return new ExternalStorageDatabaseConnection(connection, _externalStorageConnectionProvider.GetProvider());
        }

        private IExternalStorageConnectionProvider CreateExternalStorageProviderFromConfigurationSettings(IDictionary<string, string> settings)
        {
            Type providerType;
            IExternalStorageConnectionProvider externalStorageConnectionProvider = null;
            if (settings.TryGetValue(ExternalStorageParameters.ConnectionProviderProperty, out string connectionProviderAsString) && connectionProviderAsString != null)
            {
                providerType = Type.GetType(connectionProviderAsString);
                externalStorageConnectionProvider = (IExternalStorageConnectionProvider)Activator.CreateInstance(providerType);
                externalStorageConnectionProvider.Configure(settings);
            }
            else if (settings.TryGetValue(ExternalStorageParameters.ConnectionStringNameProperty, out string connectionStringName) && connectionStringName != null)
            {
                ConnectionStringSettings connectionStringSettings = ConfigurationManager.ConnectionStrings[connectionStringName];
                if (connectionStringSettings != null && !string.IsNullOrEmpty(connectionStringSettings.ProviderName))
                {
                    providerType = Type.GetType(connectionStringSettings.ProviderName);
                    if (typeof(IExternalStorageConnectionProvider).IsAssignableFrom(providerType))
                    {
                        externalStorageConnectionProvider = (IExternalStorageConnectionProvider)Activator.CreateInstance(providerType);
                        externalStorageConnectionProvider.Configure(settings);
                    }
                }
            }

            return externalStorageConnectionProvider;
        }
    }
}