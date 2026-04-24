using System.Data;

namespace Portal.Infrastructure.Data;

public class UnitOfWork : IUnitOfWork
{
    public IDbConnection Connection { get; private set; }
    public IDbTransaction? Transaction { get; private set; }
    
    private bool _disposed = false;

    public UnitOfWork(IDbConnection connection)
    {
        Connection = connection;
        if (connection.State != ConnectionState.Open)
        {
            connection.Open();
        }
    }

    public void BeginTransaction()
    {
        Transaction = Connection.BeginTransaction();
    }

    public void Commit()
    {
        Transaction?.Commit();
        Dispose();
    }

    public void Rollback()
    {
        Transaction?.Rollback();
        Dispose();
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            Transaction?.Dispose();
            Connection?.Dispose();
            _disposed = true;
        }
        GC.SuppressFinalize(this);
    }
}
