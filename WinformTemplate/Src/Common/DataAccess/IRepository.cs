namespace WinformTemplate.Common.DataAccess;

public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(object id);

    Task<PagedResult<T>> QueryAsync(QueryRequest req);

    Task<T> AddAsync(T entity);

    Task<bool> UpdateAsync(T entity);

    Task<bool> DeleteAsync(object id);
}
