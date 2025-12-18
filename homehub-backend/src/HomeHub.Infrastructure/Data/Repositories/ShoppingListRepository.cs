using HomeHub.Application.Common.Pagination;
using HomeHub.Application.Features.ShoppingList.Interfaces;
using ShoppingListEntity = HomeHub.Domain.Entities.ShoppingList;
using HomeHub.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace HomeHub.Infrastructure.Data.Repositories;

public class ShoppingListRepository(HomeHubContext context) : RepositoryBase<ShoppingListEntity>(context), IShoppingListRepository
{
    public async Task<ShoppingListEntity?> GetByIdWithItemsAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _context.Set<ShoppingListEntity>()
            .Include(sl => sl.Items)
                .ThenInclude(item => item.InventoryItem)
            .FirstOrDefaultAsync(sl => sl.Id == id, cancellationToken);
    }

    public async Task<PaginationResult<ShoppingListEntity>> GetPaginatedWithItemsAsync(int pageNumber, int pageSize, CancellationToken cancellationToken)
    {
        var query = _context.Set<ShoppingListEntity>()
            .Include(sl => sl.Items)
                .ThenInclude(item => item.InventoryItem)
            .AsQueryable();

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PaginationResult<ShoppingListEntity>(items, totalCount, pageNumber, pageSize);
    }
}
