using System.Data;

namespace Portal.Infra {
    public interface IUnitOfWork : IDisposable {
        IDbConnection Connection { get; }
        IDbTransaction? Transaction { get; }
        void Begin();
        void Commit();
        void Rollback();
    }
}