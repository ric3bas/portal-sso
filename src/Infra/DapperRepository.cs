using Dapper;

namespace Portal.Infra {
    public abstract class DapperRepository<T> : IDapperRepository<T> {
        protected readonly IUnitOfWork _unitOfWork;

        protected DapperRepository(IUnitOfWork unitOfWork) {
            _unitOfWork = unitOfWork;
        }

        public virtual IEnumerable<T> Query(string sql, object? param) {
            return _unitOfWork.Connection.Query<T>(sql, param, _unitOfWork.Transaction);
        }

        public virtual T? QuerySingle(string sql, object? param) {
            return _unitOfWork.Connection.QuerySingleOrDefault<T>(sql, param, _unitOfWork.Transaction);
        }

        public virtual int Execute(string sql, object? param) {
            return _unitOfWork.Connection.Execute(sql, param, _unitOfWork.Transaction);
        }
    }
}