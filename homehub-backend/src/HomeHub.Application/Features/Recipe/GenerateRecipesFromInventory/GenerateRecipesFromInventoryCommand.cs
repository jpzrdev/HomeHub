using MediatR;

namespace HomeHub.Application.Features.Recipe.GenerateRecipesFromInventory;

public record GenerateRecipesFromInventoryCommand(
    List<Guid> InventoryItemIds,
    string? UserDescription) : IRequest<List<GeneratedRecipeResponse>>;
