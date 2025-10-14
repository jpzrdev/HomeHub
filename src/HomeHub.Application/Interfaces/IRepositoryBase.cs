using System.Linq.Expressions;
using HomeHub.Application.Common.Pagination;

namespace HomeHub.Application.Interfaces;

public interface IRepositoryBase<T> where T : class
{
    Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken);
    Task<PaginationResult<T>> GetPaginatedAsync(
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken,
        Expression<Func<T, bool>>? predicate = null,
        Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
        Func<IQueryable<T>, IQueryable<T>>? include = null
    );
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken);
    Task AddAsync(T entity, CancellationToken cancellationToken);
    Task UpdateAsync(T entity, CancellationToken cancellationToken);
    Task RemoveAsync(T entity, CancellationToken cancellationToken);
}