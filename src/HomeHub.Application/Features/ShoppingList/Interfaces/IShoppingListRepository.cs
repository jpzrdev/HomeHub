using HomeHub.Application.Common.Pagination;
using HomeHub.Application.Interfaces;
using ShoppingListEntity = HomeHub.Domain.Entities.ShoppingList;

namespace HomeHub.Application.Features.ShoppingList.Interfaces;

public interface IShoppingListRepository : IRepositoryBase<ShoppingListEntity>
{
    Task<ShoppingListEntity?> GetByIdWithItemsAsync(Guid id, CancellationToken cancellationToken);
    Task<PaginationResult<ShoppingListEntity>> GetPaginatedWithItemsAsync(int pageNumber, int pageSize, CancellationToken cancellationToken);
}
