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
        return await GetByIdAsync(id, query => query.Include(sl => sl.Items), cancellationToken);
    }

    public async Task<PaginationResult<ShoppingListEntity>> GetPaginatedWithItemsAsync(int pageNumber, int pageSize, CancellationToken cancellationToken)
    {
        return await GetPaginatedAsync(pageNumber, pageSize, cancellationToken, include: query => query.Include(sl => sl.Items));
    }
}
