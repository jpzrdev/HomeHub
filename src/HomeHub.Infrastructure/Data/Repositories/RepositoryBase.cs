using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using HomeHub.Infrastructure.Data;
using HomeHub.Application.Interfaces;

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

    public async Task<T?> GetByIdAsync(Guid id)
    {
        return await _dbSet.FindAsync(id);
    }

    public async Task<IEnumerable<T>> GetAllAsync()
    {
        return await _dbSet.ToListAsync();
    }

    public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
    {
        return await _dbSet.Where(predicate).ToListAsync();
    }

    public async Task AddAsync(T entity)
    {
        await _dbSet.AddAsync(entity);
        await _context.SaveChangesAsync();
    }

    public void Update(T entity)
    {
        _dbSet.Update(entity);
        _context.SaveChanges();
    }

    public void Remove(T entity)
    {
        _dbSet.Remove(entity);
        _context.SaveChanges();
    }
}
