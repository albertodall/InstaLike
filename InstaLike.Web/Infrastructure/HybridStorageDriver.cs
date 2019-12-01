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
    internal class HybridStorageDriver : IDriver
    {
        public HybridStorageDriver(IDriver driver)
        {
            Driver = driver ?? throw new ArgumentNullException(nameof(driver));
        }

        public bool SupportsMultipleOpenReaders => Driver.SupportsMultipleOpenReaders;

        public bool SupportsMultipleQueries => Driver.SupportsMultipleQueries;

        public bool RequiresTimeSpanForTime => Driver.RequiresTimeSpanForTime;

        public bool SupportsSystemTransactions => Driver.SupportsSystemTransactions;

        public bool SupportsNullEnlistment => Driver.SupportsNullEnlistment;

        public bool SupportsEnlistmentWhenAutoEnlistmentIsDisabled => Driver.SupportsEnlistmentWhenAutoEnlistmentIsDisabled;

        public bool HasDelayedDistributedTransactionCompletion => Driver.HasDelayedDistributedTransactionCompletion;

        public DateTime MinDate => Driver.MinDate;

        public IDriver Driver { get; }

        public void Configure(IDictionary<string, string> settings)
        {
            Driver.Configure(settings);
        }

        public DbConnection CreateConnection()
        {
            return Driver.CreateConnection();
        }

        public void ExpandQueryParameters(DbCommand cmd, SqlString sqlString, SqlType[] parameterTypes)
        {
            Driver.ExpandQueryParameters(cmd, sqlString, parameterTypes);
        }

        public void AdjustCommand(DbCommand command)
        {
            Driver.AdjustCommand(command);
        }

        public DbCommand GenerateCommand(CommandType type, SqlString sqlString, SqlType[] parameterTypes)
        {
            // External storage doesn't have a "real" connection...
            return new HybridStorageCommand(null, Driver.GenerateCommand(type, sqlString, parameterTypes));
        }

        public IResultSetsCommand GetResultSetsCommand(ISessionImplementor session)
        {
            return Driver.GetResultSetsCommand(session);
        }

        public void PrepareCommand(DbCommand command)
        {
            Driver.PrepareCommand(GetUnderlyingCommand(command));
        }

        public DbParameter GenerateParameter(DbCommand command, string name, SqlType sqlType)
        {
            return Driver.GenerateParameter(command, name, sqlType);
        }

        public void RemoveUnusedCommandParameters(DbCommand cmd, SqlString sqlString)
        {
            Driver.RemoveUnusedCommandParameters(GetUnderlyingCommand(cmd), sqlString);
        }

        private static DbCommand GetUnderlyingCommand(DbCommand command)
        {
            return command is HybridStorageCommand cmd
                ? cmd.UnderlyingCommand
                : command;
        }
    }
}