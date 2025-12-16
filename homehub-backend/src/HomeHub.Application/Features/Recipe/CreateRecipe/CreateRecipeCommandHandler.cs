using HomeHub.Application.Features.Inventory.Interfaces;
using HomeHub.Application.Features.Recipe.Interfaces;
using RecipeEntity = HomeHub.Domain.Entities.Recipe;
using MediatR;

namespace HomeHub.Application.Features.Recipe.CreateRecipe;

public class CreateRecipeCommandHandler(
    IRecipeRepository recipeRepository,
    IInventoryItemRepository inventoryItemRepository
) : IRequestHandler<CreateRecipeCommand, Guid>
{
    public async Task<Guid> Handle(CreateRecipeCommand request, CancellationToken cancellationToken)
    {
        var recipe = new RecipeEntity(request.Title, request.Description);

        // Add steps in the specified order
        foreach (var stepDto in request.Steps)
        {
            recipe.AddStep(stepDto.Order, stepDto.Description);
        }

        // Validate and add ingredients
        foreach (var ingredientDto in request.Ingredients)
        {
            // Verify that the inventory item exists
            var inventoryItem = await inventoryItemRepository.GetByIdAsync(
                ingredientDto.InventoryItemId,
                cancellationToken);

            if (inventoryItem == null)
                throw new InvalidOperationException(
                    $"Inventory item with ID {ingredientDto.InventoryItemId} not found.");

            recipe.AddIngredient(ingredientDto.InventoryItemId, ingredientDto.Quantity);
        }

        await recipeRepository.AddAsync(recipe, cancellationToken);

        return recipe.Id;
    }
}

