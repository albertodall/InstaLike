using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using InstaLike.Web.Services;
using NHibernate.Connection;
using NHibernate.Driver;

namespace InstaLike.Web.Infrastructure
{
    internal class ExternalStorageDriverConnectionProvider : IConnectionProvider
    {
        private readonly IConnectionProvider _connectionProvider;
        private IExternalStorageProviderFactory _externalStorageProviderFactory;

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
            _externalStorageProviderFactory = new AzureBlobStorageProviderFactory("UseDevelopmentStorage=true");
            _connectionProvider.Configure(settings);
        }

        public void Dispose()
        {
            _connectionProvider.Dispose();
        }

        public DbConnection GetConnection()
        {
            var connection = _connectionProvider.GetConnection();
            if (_externalStorageProviderFactory == null)
            {
                return connection;
            }

            return new ExternalStorageDatabaseConnection(connection, _externalStorageProviderFactory.GetProvider());
        }

        public async Task<DbConnection> GetConnectionAsync(CancellationToken cancellationToken)
        {
            var connection = await _connectionProvider.GetConnectionAsync(cancellationToken);
            if (_externalStorageProviderFactory == null)
            {
                return connection;
            }

            return new ExternalStorageDatabaseConnection(connection, _externalStorageProviderFactory.GetProvider());
        }

        private IExternalStorageProviderFactory CreateExternalStorageProviderFromConfigurationSettings(IDictionary<string, string> settings)
        {
            Type providerType;
            IExternalStorageProviderFactory factory = null;
            if (settings.TryGetValue(ExternalStorageParameters.ConnectionProviderProperty, out string connectionProviderAsString) && connectionProviderAsString != null)
            {
                providerType = Type.GetType(connectionProviderAsString);
                factory = (IExternalStorageProviderFactory)Activator.CreateInstance(providerType);
                factory.Configure(settings);
            }
            else if (settings.TryGetValue(ExternalStorageParameters.ConnectionStringNameProperty, out string connectionStringName) && connectionStringName != null)
            {
                ConnectionStringSettings connectionStringSettings = ConfigurationManager.ConnectionStrings[connectionStringName];
                if (connectionStringSettings != null && !string.IsNullOrEmpty(connectionStringSettings.ProviderName))
                {
                    providerType = Type.GetType(connectionStringSettings.ProviderName);
                    if (typeof(IExternalStorageProviderFactory).IsAssignableFrom(providerType))
                    {
                        factory = (IExternalStorageProviderFactory)Activator.CreateInstance(providerType);
                        factory.Configure(settings);
                    }
                }
            }

            return factory;
        }
    }

    internal class AzureBlobStorageProviderFactory : IExternalStorageProviderFactory
    {
        private readonly string _storageConnectionString;

        public AzureBlobStorageProviderFactory(string storageConnectionString)
        {
            _storageConnectionString = storageConnectionString;
        }

        public void Configure(IDictionary<string, string> settings)
        {
        
        }

        public IExternalStoragePictureProvider GetProvider()
        {
            return new AzureBlobStoragePictureProvider(_storageConnectionString);
        }
    }
}