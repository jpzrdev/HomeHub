using HomeHub.Application.Features.Inventory.Interfaces;
using HomeHub.Application.Features.ShoppingList.Interfaces;
using HomeHub.Domain.Entities;
using MediatR;

namespace HomeHub.Application.Features.ShoppingList.GenerateShoppingList;

public class GenerateShoppingListCommandHandler(
    IInventoryItemRepository inventoryItemRepository,
    IShoppingListRepository shoppingListRepository
) : IRequestHandler<GenerateShoppingListCommand, Guid>
{
    public async Task<Guid> Handle(GenerateShoppingListCommand request, CancellationToken cancellationToken)
    {
        // Find all inventory items where QuantityAvailable < MinimumQuantity
        var itemsBelowMinimum = await inventoryItemRepository.FindAsync(
            item => item.QuantityAvailable < item.MinimumQuantity,
            cancellationToken
        );

        // Create shopping list first to get its Id
        var shoppingList = new Domain.Entities.ShoppingList(Enumerable.Empty<ShoppingListItem>());

        // Create shopping list items with the shopping list Id
        var shoppingListItems = itemsBelowMinimum.Select(item =>
        {
            var quantityToBuy = item.MinimumQuantity - item.QuantityAvailable;
            return new ShoppingListItem(
                shoppingListId: shoppingList.Id,
                inventoryItemId: item.Id,
                quantityToBuy: quantityToBuy
            );
        }).ToList();

        // Add items to the shopping list
        foreach (var item in shoppingListItems)
        {
            shoppingList.AddItem(item);
        }

        await shoppingListRepository.AddAsync(shoppingList, cancellationToken);

        return shoppingList.Id;
    }
}
