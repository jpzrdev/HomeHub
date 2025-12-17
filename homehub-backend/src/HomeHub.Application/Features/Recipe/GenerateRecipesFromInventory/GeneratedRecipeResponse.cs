namespace HomeHub.Application.Features.Recipe.GenerateRecipesFromInventory;

public record GeneratedRecipeResponse(
    string Title,
    string Description,
    List<GeneratedRecipeStepResponse> Steps,
    List<GeneratedRecipeIngredientResponse> Ingredients);

public record GeneratedRecipeStepResponse(int Order, string Description);

public record GeneratedRecipeIngredientResponse(Guid InventoryItemId, string InventoryItemName, decimal Quantity);
