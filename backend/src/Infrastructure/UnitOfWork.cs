using System.Data;

namespace Portal.Infrastructure {
    public class UnitOfWork(IDbConnection connection) : IUnitOfWork {
        public IDbConnection Connection { get; private set; } = connection;
        public IDbTransaction? Transaction { get; private set; }

        public void Begin() {
            if (Connection.State != ConnectionState.Open)
                Connection.Open();
            Transaction = Connection.BeginTransaction();
        }

        public void Commit() {
            Transaction?.Commit();
            Transaction?.Dispose();
            Transaction = null;
        }

        public void Rollback() {
            Transaction?.Rollback();
            Transaction?.Dispose();
            Transaction = null;
        }

        public void Dispose() {
            Transaction?.Dispose();
            Connection?.Dispose();
        }
    }
}
