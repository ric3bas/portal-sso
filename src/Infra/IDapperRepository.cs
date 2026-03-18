namespace Portal.Infra {
    public interface IDapperRepository<T> {
        IEnumerable<T> Query(string sql, object? param);
        T? QuerySingle(string sql, object? param);
        int Execute(string sql, object? param);
    }
}