using HomeHub.Application.Interfaces;
using ShoppingListEntity = HomeHub.Domain.Entities.ShoppingList;

namespace HomeHub.Application.Features.ShoppingList.Interfaces;

public interface IShoppingListRepository : IRepositoryBase<ShoppingListEntity>
{ }
