using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using HomeHub.Infrastructure.Data;
using HomeHub.Application.Interfaces;
using HomeHub.Application.Common.Pagination;
using HomeHub.Domain.Common;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace HomeHub.Infrastructure.Repositories;

public class RepositoryBase<T> : IRepositoryBase<T> where T : class
{
    protected readonly HomeHubContext _context;
    protected readonly DbSet<T> _dbSet;

    public RepositoryBase(HomeHubContext context)
    {
        _context = context;
        _dbSet = _context.Set<T>();
    }

    public async Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _dbSet.FindAsync(id, cancellationToken);
    }

    public async Task<T?> GetByIdAsync(Guid id, Func<IQueryable<T>, IQueryable<T>>? include, CancellationToken cancellationToken)
    {
        if (include == null)
        {
            return await GetByIdAsync(id, cancellationToken);
        }

        IQueryable<T> query = _dbSet;

        query = include(query);

        // Use EF.Property to access the Id property when T doesn't have a direct constraint
        return await query.FirstOrDefaultAsync(e => EF.Property<Guid>(e, "Id") == id, cancellationToken);
    }

    public async Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await _dbSet.ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken)
    {
        return await _dbSet.Where(predicate).ToListAsync(cancellationToken);
    }

    public async Task AddAsync(T entity, CancellationToken cancellationToken)
    {
        await _dbSet.AddAsync(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(T entity, CancellationToken cancellationToken)
    {
        _dbSet.Update(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task RemoveAsync(T entity, CancellationToken cancellationToken)
    {
        _dbSet.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }



    public async Task<PaginationResult<T>> GetPaginatedAsync(int pageNumber, int pageSize, CancellationToken cancellationToken, Expression<Func<T, bool>>? predicate = null, Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null, Func<IQueryable<T>, IQueryable<T>>? include = null)
    {
        IQueryable<T> query = _dbSet.AsNoTracking();

        if (include != null)
            query = include(query);

        if (predicate != null)
            query = query.Where(predicate);

        var totalItems = await query.CountAsync();

        if (orderBy != null)
            query = orderBy(query);

        query = query.Skip((pageNumber - 1) * pageSize).Take(pageSize);

        var items = await query.ToListAsync();

        return new PaginationResult<T>(items, totalItems, pageNumber, pageSize);
    }
}
