using HomeHub.Application.Features.Inventory.Interfaces;
using HomeHub.Domain.Entities;
using HomeHub.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace HomeHub.Infrastructure.Data.Repositories;

public class InventoryItemRepository(HomeHubContext context) : RepositoryBase<InventoryItem>(context), IInventoryRepository
{
}