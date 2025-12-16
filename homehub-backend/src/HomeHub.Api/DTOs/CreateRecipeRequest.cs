namespace HomeHub.Api.DTOs;

public record RecipeStepRequest(int Order, string Description);

public record RecipeIngredientRequest(Guid InventoryItemId, decimal Quantity);

public record CreateRecipeRequest(
    string Title,
    string Description,
    List<RecipeStepRequest> Steps,
    List<RecipeIngredientRequest> Ingredients);

