using HomeHub.Domain.Entities;
using MediatR;

namespace HomeHub.Application.Features.Inventory.UpdateInventoryItem;

public record UpdateInventoryItemCommand(
    Guid Id,
    string? Name,
    decimal? QuantityAvailable,
    decimal? MinimumQuantity) : IRequest<InventoryItem>
{

}