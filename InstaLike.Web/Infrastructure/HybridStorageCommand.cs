using System.Data;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace InstaLike.Web.Infrastructure
{
    internal class HybridStorageCommand : DbCommand
    {
        private DbConnection _connection;
        private readonly DbCommand _command;

        public HybridStorageCommand(DbConnection connection, DbCommand command)
        {
            _connection = connection;
            _command = command;
        }

        public virtual DbCommand UnderlyingCommand
        {
            get { return _command; }
        }

        [SuppressMessage("Security", "CA2100:Review SQL queries for security vulnerabilities", 
            Justification = "It's a matter of NHibernate behind the scenes.")]
        public override string CommandText
        {
            get => _command.CommandText;
            set => _command.CommandText = value;
        }

        public override int CommandTimeout
        {
            get => _command.CommandTimeout;
            set => _command.CommandTimeout = value;
        }

        public override CommandType CommandType
        {
            get => _command.CommandType;
            set => _command.CommandType = value;
        }

        public override bool DesignTimeVisible
        {
            get => false;
            set { }
        }

        public override UpdateRowSource UpdatedRowSource
        {
            get => _command.UpdatedRowSource;
            set => _command.UpdatedRowSource = value;
        }

        protected override DbConnection DbConnection
        {
            get => _connection;
            set
            {
                _connection = value;
                if (value is HybridStorageConnection)
                {
                    _command.Connection = ((HybridStorageConnection)value).DatabaseConnection;
                }
                else
                {
                    _command.Connection = value;
                }
            } 
        }

        protected override DbParameterCollection DbParameterCollection => _command.Parameters;

        protected override DbTransaction DbTransaction
        {
            get => _command.Transaction;
            set => _command.Transaction = value;
        }

        public override void Cancel()
        {
            _command.Cancel();
        }

        public override int ExecuteNonQuery()
        {
            return _command.ExecuteNonQuery();
        }

        public override Task<int> ExecuteNonQueryAsync(CancellationToken cancellationToken)
        {
            return _command.ExecuteNonQueryAsync(cancellationToken);
        }

        public override object ExecuteScalar()
        {
            return _command.ExecuteScalar();
        }

        public override Task<object> ExecuteScalarAsync(CancellationToken cancellationToken)
        {
            return _command.ExecuteScalarAsync(cancellationToken);
        }

        public override void Prepare()
        {
            _command.Prepare();
        }

        protected override DbParameter CreateDbParameter()
        {
            return _command.CreateParameter();
        }

        protected override DbDataReader ExecuteDbDataReader(CommandBehavior behavior)
        {
            return _command.ExecuteReader(behavior);
        }

        protected override Task<DbDataReader> ExecuteDbDataReaderAsync(CommandBehavior behavior, CancellationToken cancellationToken)
        {
            return _command.ExecuteReaderAsync(behavior, cancellationToken);
        }
    }
}