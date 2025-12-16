namespace HomeHub.Api.DTOs;

public record UpdateInventoryItemRequest(
    string? Name,
    decimal? QuantityAvailable,
    decimal? MinimumQuantity);
