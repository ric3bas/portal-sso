using Portal.Domain.Common;
using System.Data;
using Dapper;

namespace Portal.Infrastructure.Data;

public abstract class DapperRepository
{
    protected readonly IUnitOfWork _unitOfWork;

    protected DapperRepository(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    protected async Task<IEnumerable<T>> QueryAsync<T>(string sql, object? parameters = null)
    {
        return await _unitOfWork.Connection.QueryAsync<T>(sql, parameters, _unitOfWork.Transaction);
    }

    protected async Task<T?> QuerySingleAsync<T>(string sql, object? parameters = null)
    {
        return await _unitOfWork.Connection.QuerySingleOrDefaultAsync<T>(sql, parameters, _unitOfWork.Transaction);
    }

    protected async Task<int> ExecuteAsync(string sql, object? parameters = null)
    {
        return await _unitOfWork.Connection.ExecuteAsync(sql, parameters, _unitOfWork.Transaction);
    }

    protected async Task<T?> ExecuteScalarAsync<T>(string sql, object? parameters = null)
    {
        return await _unitOfWork.Connection.ExecuteScalarAsync<T>(sql, parameters, _unitOfWork.Transaction);
    }
}
