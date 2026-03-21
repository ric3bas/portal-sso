using Dapper;

namespace Portal.Infra {
    public interface IDapperRepository
    {
        IEnumerable<T> Query<T>(string sql, object? param = null, int? timeout = null);
        T? QuerySingle<T>(string sql, object? param = null, int? timeout = null);
        int Execute(string sql, object? param = null, int? timeout = null);

        Task<IEnumerable<T>> QueryAsync<T>(string sql, object? param = null, int? timeout = null);
        Task<T?> QuerySingleAsync<T>(string sql, object? param = null, int? timeout = null);
        Task<int> ExecuteAsync(string sql, object? param = null, int? timeout = null);
        Task<SqlMapper.GridReader> QueryMultipleAsync(string sql, object? param = null, int? timeout = null);
    }
}
