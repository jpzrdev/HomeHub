using HomeHub.Application.Features.ShoppingList.Interfaces;
using ShoppingListEntity = HomeHub.Domain.Entities.ShoppingList;
using HomeHub.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace HomeHub.Infrastructure.Data.Repositories;

public class ShoppingListRepository(HomeHubContext context) : RepositoryBase<ShoppingListEntity>(context), IShoppingListRepository
{
}
