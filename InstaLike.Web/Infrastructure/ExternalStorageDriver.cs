using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using NHibernate.Driver;
using NHibernate.Engine;
using NHibernate.SqlCommand;
using NHibernate.SqlTypes;

namespace InstaLike.Web.Infrastructure
{
    internal class ExternalStorageDriver : IDriver
    {
        private readonly IDriver _driver;

        public ExternalStorageDriver(IDriver driver)
        {
            _driver = driver ?? throw new ArgumentNullException(nameof(driver));
        }

        public bool SupportsMultipleOpenReaders => _driver.SupportsMultipleOpenReaders;

        public bool SupportsMultipleQueries => _driver.SupportsMultipleQueries;

        public bool RequiresTimeSpanForTime => _driver.RequiresTimeSpanForTime;

        public bool SupportsSystemTransactions => _driver.SupportsSystemTransactions;

        public bool SupportsNullEnlistment => _driver.SupportsNullEnlistment;

        public bool SupportsEnlistmentWhenAutoEnlistmentIsDisabled => _driver.SupportsEnlistmentWhenAutoEnlistmentIsDisabled;

        public bool HasDelayedDistributedTransactionCompletion => _driver.HasDelayedDistributedTransactionCompletion;

        public DateTime MinDate => _driver.MinDate;

        public IDriver UnderlyingDriver => _driver;

        public void Configure(IDictionary<string, string> settings)
        {
            _driver.Configure(settings);
        }

        public DbConnection CreateConnection()
        {
            return _driver.CreateConnection();
        }

        public void ExpandQueryParameters(DbCommand cmd, SqlString sqlString, SqlType[] parameterTypes)
        {
            _driver.ExpandQueryParameters(cmd, sqlString, parameterTypes);
        }

        public void AdjustCommand(DbCommand command)
        {
            _driver.AdjustCommand(command);
        }

        public DbCommand GenerateCommand(CommandType type, SqlString sqlString, SqlType[] parameterTypes)
        {
            // External storage doesn't have a "real" connection...
            return new ExternalStorageDatabaseCommand(null, _driver.GenerateCommand(type, sqlString, parameterTypes));
        }

        public IResultSetsCommand GetResultSetsCommand(ISessionImplementor session)
        {
            return _driver.GetResultSetsCommand(session);
        }

        public void PrepareCommand(DbCommand command)
        {
            _driver.PrepareCommand(GetUnderlyingCommand(command));
        }

        public DbParameter GenerateParameter(DbCommand command, string name, SqlType sqlType)
        {
            return _driver.GenerateParameter(command, name, sqlType);
        }

        public void RemoveUnusedCommandParameters(DbCommand cmd, SqlString sqlString)
        {
            _driver.RemoveUnusedCommandParameters(GetUnderlyingCommand(cmd), sqlString);
        }

        private static DbCommand GetUnderlyingCommand(DbCommand externalStorageDatabaseCommand)
        {
            return externalStorageDatabaseCommand is ExternalStorageDatabaseCommand innerCommand
                ? innerCommand.UnderlyingCommand
                : externalStorageDatabaseCommand;
        }
    }
}