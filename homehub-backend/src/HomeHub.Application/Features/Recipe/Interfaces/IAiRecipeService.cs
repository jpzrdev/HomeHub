namespace HomeHub.Application.Features.Recipe.Interfaces;

public record GeneratedRecipe(
    string Title,
    string Description,
    List<GeneratedRecipeStep> Steps,
    List<GeneratedRecipeIngredient> Ingredients);

public record GeneratedRecipeStep(int Order, string Description);

public record GeneratedRecipeIngredient(string InventoryItemName, decimal Quantity);

public interface IAiRecipeService
{
    Task<List<GeneratedRecipe>> GenerateRecipesAsync(
        List<string> inventoryItemNames,
        string? userDescription,
        string promptId,
        CancellationToken cancellationToken);
}
