using HomeHub.Application.Interfaces;
using HomeHub.Domain.Entities;

namespace HomeHub.Application.Features.Inventory.Interfaces;

public interface IInventoryItemRepository : IRepositoryBase<InventoryItem>
{
    Task DeleteWithRelatedEntitiesAsync(InventoryItem inventoryItem, CancellationToken cancellationToken);
}