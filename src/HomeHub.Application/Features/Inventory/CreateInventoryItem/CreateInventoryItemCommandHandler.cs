using HomeHub.Application.Features.Inventory.Interfaces;
using HomeHub.Domain.Entities;
using MediatR;

namespace HomeHub.Application.Features.Inventory.CreateInventory;

public class CreateInventoryItemCommandHandler(
    IInventoryItemRepository InventoryItemRepository
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

        await InventoryItemRepository.AddAsync(inventory);

        return inventory.Id;
    }
}