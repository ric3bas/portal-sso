using Dapper;

namespace Portal.Infrastructure {


    public abstract class DapperRepository : IDapperRepository
    {
        protected readonly IUnitOfWork _unitOfWork;

        protected DapperRepository(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        //private static string ResolveContext(Type type)
        //{
        //    var attr = type.GetCustomAttributes(typeof(DbContextAttribute), inherit: true).FirstOrDefault() as DbContextAttribute;
        //    if (attr == null)
        //        throw new Exception($"Classe {type.Name} não possui DbContextAttribute.");
        //    return attr.ContextName;
        //}

        public virtual IEnumerable<T> Query<T>(string sql, object? param = null, int? timeout = null)
            => _unitOfWork.Connection.Query<T>(sql, param, _unitOfWork.Transaction, commandTimeout: timeout);

        public virtual T? QuerySingle<T>(string sql, object? param = null, int? timeout = null)
            => _unitOfWork.Connection.QuerySingleOrDefault<T>(sql, param, _unitOfWork.Transaction, commandTimeout: timeout);

        public virtual int Execute(string sql, object? param = null, int? timeout = null)
            => _unitOfWork.Connection.Execute(sql, param, _unitOfWork.Transaction, commandTimeout: timeout);

        public virtual async Task<IEnumerable<T>> QueryAsync<T>(string sql, object? param = null, int? timeout = null)
            => await _unitOfWork.Connection.QueryAsync<T>(sql, param, _unitOfWork.Transaction, commandTimeout: timeout);

        public virtual async Task<T?> QuerySingleAsync<T>(string sql, object? param = null, int? timeout = null)
            => await _unitOfWork.Connection.QuerySingleOrDefaultAsync<T>(sql, param, _unitOfWork.Transaction, commandTimeout: timeout);

        public virtual async Task<int> ExecuteAsync(string sql, object? param = null, int? timeout = null)
            => await _unitOfWork.Connection.ExecuteAsync(sql, param, _unitOfWork.Transaction, commandTimeout: timeout);

        public async Task<SqlMapper.GridReader> QueryMultipleAsync(string sql, object? param = null, int? timeout = null)
            => await _unitOfWork.Connection.QueryMultipleAsync(sql, param, _unitOfWork.Transaction, commandTimeout: timeout);
    }
}
