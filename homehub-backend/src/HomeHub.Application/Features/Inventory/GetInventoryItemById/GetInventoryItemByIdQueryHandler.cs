using HomeHub.Application.Features.Inventory.Interfaces;
using HomeHub.Domain.Entities;
using MediatR;

namespace HomeHub.Application.Features.Inventory.GetInventoryItemById;

public class GetInventoryItemByIdQueryHandler(IInventoryItemRepository repository) : IRequestHandler<GetInventoryItemByIdQuery, InventoryItem>
{
    public async Task<InventoryItem> Handle(GetInventoryItemByIdQuery request, CancellationToken cancellationToken)
    {
        var inventoryItem = await repository.GetByIdAsync(request.Id, cancellationToken);

        if (inventoryItem is null)
        {
            throw new KeyNotFoundException($"Inventory item with ID {request.Id} was not found.");
        }

        return inventoryItem;
    }
}