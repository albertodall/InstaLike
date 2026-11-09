using System;
using System.Data;
using System.Data.Common;
using InstaLike.Web.Services;

namespace InstaLike.Web.Infrastructure;

/// <summary>
/// This connection "connects" the O/RM to both the database and the external storage.
/// </summary>
internal class HybridStorageConnection : DbConnection
{
    public HybridStorageConnection(DbConnection databaseConnection, IExternalStorageProvider externalStorage)
    {
        DatabaseConnection = databaseConnection ?? throw new ArgumentNullException(nameof(databaseConnection));
        ExternalStorage = externalStorage ?? throw new ArgumentNullException(nameof(externalStorage));
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

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            DatabaseConnection.Dispose();
        }
        base.Dispose(disposing);
    }

    public DbConnection DatabaseConnection { get; }

    public IExternalStorageProvider ExternalStorage { get; }
}