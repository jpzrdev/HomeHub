using HomeHub.Domain.Entities;

namespace HomeHub.Application.Features.Inventory.Interfaces;

public interface IInventoryService
{
    IEnumerable<InventoryItem> GetAll();
}