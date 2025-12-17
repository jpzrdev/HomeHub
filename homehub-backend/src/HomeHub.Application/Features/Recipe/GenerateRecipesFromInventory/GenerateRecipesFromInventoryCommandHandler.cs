using HomeHub.Application.Features.Inventory.Interfaces;
using HomeHub.Application.Features.Recipe.Interfaces;
using MediatR;

namespace HomeHub.Application.Features.Recipe.GenerateRecipesFromInventory;

public class GenerateRecipesFromInventoryCommandHandler(
    IAiRecipeService aiRecipeService,
    IInventoryItemRepository inventoryItemRepository
) : IRequestHandler<GenerateRecipesFromInventoryCommand, List<GeneratedRecipeResponse>>
{
    public async Task<List<GeneratedRecipeResponse>> Handle(GenerateRecipesFromInventoryCommand request, CancellationToken cancellationToken)
    {
        if (request.InventoryItemIds == null || request.InventoryItemIds.Count == 0)
        {
            throw new ArgumentException("At least one inventory item ID must be provided.", nameof(request));
        }

        // Fetch inventory items to get their names
        var inventoryItems = new List<Domain.Entities.InventoryItem>();
        foreach (var itemId in request.InventoryItemIds)
        {
            var item = await inventoryItemRepository.GetByIdAsync(itemId, cancellationToken);
            if (item == null)
            {
                throw new InvalidOperationException($"Inventory item with ID {itemId} not found.");
            }
            inventoryItems.Add(item);
        }

        var inventoryItemNames = inventoryItems.Select(i => i.Name).ToList();

        // Generate recipes using AI
        const string promptId = "generate-recipes-from-inventory";
        var generatedRecipes = await aiRecipeService.GenerateRecipesAsync(
            inventoryItemNames,
            request.UserDescription,
            promptId,
            cancellationToken);

        // Create a mapping of inventory item names to IDs for matching
        var nameToIdMap = inventoryItems.ToDictionary(
            i => i.Name,
            i => i.Id,
            StringComparer.OrdinalIgnoreCase);

        // Convert generated recipes to response DTOs
        var recipeResponses = new List<GeneratedRecipeResponse>();
        foreach (var generatedRecipe in generatedRecipes)
        {
            var steps = generatedRecipe.Steps
                .OrderBy(s => s.Order)
                .Select(s => new GeneratedRecipeStepResponse(s.Order, s.Description))
                .ToList();

            var ingredients = new List<GeneratedRecipeIngredientResponse>();
            foreach (var ingredient in generatedRecipe.Ingredients)
            {
                // Try to match ingredient name to inventory item (case-insensitive)
                if (nameToIdMap.TryGetValue(ingredient.InventoryItemName, out var inventoryItemId))
                {
                    var inventoryItem = inventoryItems.First(i => i.Id == inventoryItemId);
                    ingredients.Add(new GeneratedRecipeIngredientResponse(
                        inventoryItemId,
                        inventoryItem.Name,
                        ingredient.Quantity));
                }
                else
                {
                    // If ingredient name doesn't match exactly, try to find a partial match
                    var matchedItem = inventoryItems.FirstOrDefault(i =>
                        i.Name.Equals(ingredient.InventoryItemName, StringComparison.OrdinalIgnoreCase));

                    if (matchedItem != null)
                    {
                        ingredients.Add(new GeneratedRecipeIngredientResponse(
                            matchedItem.Id,
                            matchedItem.Name,
                            ingredient.Quantity));
                    }
                    // If no match found, skip this ingredient (AI might have suggested something not in inventory)
                }
            }

            recipeResponses.Add(new GeneratedRecipeResponse(
                generatedRecipe.Title,
                generatedRecipe.Description,
                steps,
                ingredients));
        }

        return recipeResponses;
    }
}
