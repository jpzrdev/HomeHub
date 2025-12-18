using HomeHub.Application.Features.Inventory.Interfaces;
using HomeHub.Domain.Entities;
using HomeHub.Infrastructure.Data;
using HomeHub.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace HomeHub.Infrastructure.Data.Repositories;

public class InventoryItemRepository(HomeHubContext context) : RepositoryBase<InventoryItem>(context), IInventoryItemRepository
{
    public async Task DeleteWithRelatedEntitiesAsync(InventoryItem inventoryItem, CancellationToken cancellationToken)
    {
        // Delete all shopping list items that reference this inventory item
        var shoppingListItems = await context.ShoppingListItems
            .Where(sli => sli.InventoryItemId == inventoryItem.Id)
            .ToListAsync(cancellationToken);

        if (shoppingListItems.Any())
        {
            context.ShoppingListItems.RemoveRange(shoppingListItems);
        }

        // Delete all recipe ingredients that reference this inventory item
        var recipeIngredients = await context.RecipeIngredients
            .Where(ri => ri.InventoryItemId == inventoryItem.Id)
            .ToListAsync(cancellationToken);

        if (recipeIngredients.Any())
        {
            context.RecipeIngredients.RemoveRange(recipeIngredients);
        }

        // Save changes for the related entities deletions
        if (shoppingListItems.Any() || recipeIngredients.Any())
        {
            await context.SaveChangesAsync(cancellationToken);
        }

        // Now delete the inventory item
        await RemoveAsync(inventoryItem, cancellationToken);
    }
}