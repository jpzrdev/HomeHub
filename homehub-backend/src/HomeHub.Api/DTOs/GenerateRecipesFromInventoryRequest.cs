namespace HomeHub.Api.DTOs;

public record GenerateRecipesFromInventoryRequest(
    List<Guid> InventoryItemIds,
    string? UserDescription);
