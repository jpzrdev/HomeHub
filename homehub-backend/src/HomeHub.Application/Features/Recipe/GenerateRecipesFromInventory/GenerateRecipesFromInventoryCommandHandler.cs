using HomeHub.Application.Features.Inventory.Interfaces;
using HomeHub.Application.Features.Recipe.Interfaces;
using MediatR;
using Microsoft.Extensions.Configuration;

namespace HomeHub.Application.Features.Recipe.GenerateRecipesFromInventory;

public class GenerateRecipesFromInventoryCommandHandler(
    IAiRecipeService aiRecipeService,
    IInventoryItemRepository inventoryItemRepository,
    IConfiguration configuration
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

        // Check if OpenAI API key is configured
        var apiKey = configuration["OpenAI:ApiKey"];
        List<GeneratedRecipe> generatedRecipes;

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            // Use mock recipes when API key is not configured
            generatedRecipes = GenerateMockRecipes(inventoryItems);
        }
        else
        {
            // Generate recipes using AI
            const string promptId = "generate-recipes-from-inventory";
            generatedRecipes = await aiRecipeService.GenerateRecipesAsync(
                inventoryItemNames,
                request.UserDescription,
                promptId,
                cancellationToken);
        }

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

    private List<GeneratedRecipe> GenerateMockRecipes(List<Domain.Entities.InventoryItem> inventoryItems)
    {
        var recipes = new List<GeneratedRecipe>();

        // Use available inventory items for mock recipes
        var items = inventoryItems.Take(3).ToList();
        if (items.Count == 0)
        {
            return recipes;
        }

        // Mock Recipe 1
        var recipe1Steps = new List<GeneratedRecipeStep>
        {
            new GeneratedRecipeStep(1, $"Prepare {items[0].Name} by washing and cutting as needed."),
            new GeneratedRecipeStep(2, $"Heat a pan over medium heat and add {items[0].Name}."),
            new GeneratedRecipeStep(3, "Cook for 5-7 minutes until tender."),
            new GeneratedRecipeStep(4, "Season to taste and serve hot.")
        };

        var recipe1Ingredients = new List<GeneratedRecipeIngredient>
        {
            new GeneratedRecipeIngredient(items[0].Name, 2.0m)
        };

        recipes.Add(new GeneratedRecipe(
            $"Simple {items[0].Name} Dish",
            $"A quick and easy recipe using {items[0].Name} from your inventory.",
            recipe1Steps,
            recipe1Ingredients));

        // Mock Recipe 2
        if (items.Count >= 2)
        {
            var recipe2Steps = new List<GeneratedRecipeStep>
            {
                new GeneratedRecipeStep(1, $"Combine {items[0].Name} and {items[1].Name} in a mixing bowl."),
                new GeneratedRecipeStep(2, "Mix well until ingredients are evenly distributed."),
                new GeneratedRecipeStep(3, "Let it rest for 10 minutes."),
                new GeneratedRecipeStep(4, "Cook or serve as desired.")
            };

            var recipe2Ingredients = new List<GeneratedRecipeIngredient>
            {
                new GeneratedRecipeIngredient(items[0].Name, 1.5m),
                new GeneratedRecipeIngredient(items[1].Name, 1.0m)
            };

            recipes.Add(new GeneratedRecipe(
                $"{items[0].Name} and {items[1].Name} Combo",
                $"A delicious combination of {items[0].Name} and {items[1].Name}.",
                recipe2Steps,
                recipe2Ingredients));
        }
        else
        {
            var recipe2Steps = new List<GeneratedRecipeStep>
            {
                new GeneratedRecipeStep(1, $"Slice {items[0].Name} into thin pieces."),
                new GeneratedRecipeStep(2, "Arrange on a serving plate."),
                new GeneratedRecipeStep(3, "Add your favorite seasonings."),
                new GeneratedRecipeStep(4, "Serve fresh.")
            };

            var recipe2Ingredients = new List<GeneratedRecipeIngredient>
            {
                new GeneratedRecipeIngredient(items[0].Name, 1.0m)
            };

            recipes.Add(new GeneratedRecipe(
                $"Fresh {items[0].Name} Salad",
                $"A refreshing dish featuring {items[0].Name}.",
                recipe2Steps,
                recipe2Ingredients));
        }

        // Mock Recipe 3
        if (items.Count >= 3)
        {
            var recipe3Steps = new List<GeneratedRecipeStep>
            {
                new GeneratedRecipeStep(1, $"Prepare {items[0].Name}, {items[1].Name}, and {items[2].Name}."),
                new GeneratedRecipeStep(2, "Heat oil in a large pan."),
                new GeneratedRecipeStep(3, $"Add {items[0].Name} and cook for 3 minutes."),
                new GeneratedRecipeStep(4, $"Add {items[1].Name} and {items[2].Name}, cook for another 5 minutes."),
                new GeneratedRecipeStep(5, "Season and serve.")
            };

            var recipe3Ingredients = new List<GeneratedRecipeIngredient>
            {
                new GeneratedRecipeIngredient(items[0].Name, 1.0m),
                new GeneratedRecipeIngredient(items[1].Name, 1.0m),
                new GeneratedRecipeIngredient(items[2].Name, 0.5m)
            };

            recipes.Add(new GeneratedRecipe(
                $"Mixed {items[0].Name}, {items[1].Name}, and {items[2].Name} Medley",
                $"A hearty combination of three ingredients from your inventory.",
                recipe3Steps,
                recipe3Ingredients));
        }
        else if (items.Count == 2)
        {
            var recipe3Steps = new List<GeneratedRecipeStep>
            {
                new GeneratedRecipeStep(1, $"Layer {items[0].Name} and {items[1].Name} in a baking dish."),
                new GeneratedRecipeStep(2, "Bake at 350°F for 20 minutes."),
                new GeneratedRecipeStep(3, "Let cool slightly before serving.")
            };

            var recipe3Ingredients = new List<GeneratedRecipeIngredient>
            {
                new GeneratedRecipeIngredient(items[0].Name, 2.0m),
                new GeneratedRecipeIngredient(items[1].Name, 1.5m)
            };

            recipes.Add(new GeneratedRecipe(
                $"Baked {items[0].Name} and {items[1].Name}",
                $"A warm, comforting dish using both ingredients.",
                recipe3Steps,
                recipe3Ingredients));
        }
        else
        {
            var recipe3Steps = new List<GeneratedRecipeStep>
            {
                new GeneratedRecipeStep(1, $"Dice {items[0].Name} into small cubes."),
                new GeneratedRecipeStep(2, "Sauté in a pan with oil."),
                new GeneratedRecipeStep(3, "Cook until golden brown."),
                new GeneratedRecipeStep(4, "Garnish and serve.")
            };

            var recipe3Ingredients = new List<GeneratedRecipeIngredient>
            {
                new GeneratedRecipeIngredient(items[0].Name, 1.5m)
            };

            recipes.Add(new GeneratedRecipe(
                $"Sautéed {items[0].Name}",
                $"A classic preparation of {items[0].Name}.",
                recipe3Steps,
                recipe3Ingredients));
        }

        return recipes;
    }
}
