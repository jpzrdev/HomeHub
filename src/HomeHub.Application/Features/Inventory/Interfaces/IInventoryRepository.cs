using HomeHub.Application.Interfaces;
using HomeHub.Domain.Entities;

namespace HomeHub.Application.Features.Inventory.Interfaces;

public interface IInventoryRepository : IRepositoryBase<InventoryItem>
{ }