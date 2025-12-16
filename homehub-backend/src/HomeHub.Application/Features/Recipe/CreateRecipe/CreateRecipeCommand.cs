using MediatR;

namespace HomeHub.Application.Features.Recipe.CreateRecipe;

public record RecipeStepDto(int Order, string Description);

public record RecipeIngredientDto(Guid InventoryItemId, decimal Quantity);

public record CreateRecipeCommand(
    string Title,
    string Description,
    List<RecipeStepDto> Steps,
    List<RecipeIngredientDto> Ingredients) : IRequest<Guid>;

