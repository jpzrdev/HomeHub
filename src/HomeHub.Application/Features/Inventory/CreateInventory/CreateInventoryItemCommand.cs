using MediatR;

namespace HomeHub.Application.Features.Inventory.CreateInventory;

public record CreateInventoryItemCommand(
    string Name,
    decimal QuantityAvailable,
    decimal MinimumQuantity,
    bool NotifyOnBelowMinimumQuantity) : IRequest<Guid>;