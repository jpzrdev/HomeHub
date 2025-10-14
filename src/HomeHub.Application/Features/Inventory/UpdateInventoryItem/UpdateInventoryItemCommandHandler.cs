using HomeHub.Application.Features.Inventory.Interfaces;
using HomeHub.Domain.Entities;
using MediatR;

namespace HomeHub.Application.Features.Inventory.UpdateInventoryItem;

public class UpdateInventoryItemCommandHandler(IInventoryItemRepository repository) : IRequestHandler<UpdateInventoryItemCommand, InventoryItem>
{
    public async Task<InventoryItem> Handle(UpdateInventoryItemCommand request, CancellationToken cancellationToken)
    {
        var inventoryItem = await repository.GetByIdAsync(request.Id, cancellationToken);

        if (inventoryItem is null)
        {
            return null;
        }

        inventoryItem.Update(request.Name, request.QuantityAvailable, request.MinimumQuantity, request.NotifyOnBelowMinimumQuantity);

        await repository.UpdateAsync(inventoryItem, cancellationToken);

        return inventoryItem;
    }
}