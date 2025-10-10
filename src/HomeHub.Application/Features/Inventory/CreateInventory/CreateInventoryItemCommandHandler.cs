using HomeHub.Application.Features.Inventory.Interfaces;
using HomeHub.Domain.Entities;
using MediatR;

namespace HomeHub.Application.Features.Inventory.CreateInventory;

public class CreateInventoryItemCommandHandler(
    IInventoryRepository inventoryRepository
) : IRequestHandler<CreateInventoryItemCommand, Guid>
{
    public async Task<Guid> Handle(CreateInventoryItemCommand request, CancellationToken cancellationToken)
    {
        var inventory = new InventoryItem(
            request.Name,
            request.QuantityAvailable,
            request.MinimumQuantity,
            request.NotifyOnBelowMinimumQuantity
        );

        await inventoryRepository.AddAsync(inventory);

        return inventory.Id;
    }
}