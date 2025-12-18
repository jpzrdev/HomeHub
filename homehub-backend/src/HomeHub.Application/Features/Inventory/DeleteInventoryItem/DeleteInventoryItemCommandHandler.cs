using HomeHub.Application.Features.Inventory.Interfaces;
using MediatR;

namespace HomeHub.Application.Features.Inventory.DeleteInventoryItem;

public class DeleteInventoryItemCommandHandler(IInventoryItemRepository repository)
    : IRequestHandler<DeleteInventoryItemCommand, Unit>
{
    public async Task<Unit> Handle(DeleteInventoryItemCommand request, CancellationToken cancellationToken)
    {
        var inventoryItem = await repository.GetByIdAsync(request.Id, cancellationToken);

        if (inventoryItem is null)
        {
            throw new KeyNotFoundException($"Inventory item with ID {request.Id} was not found.");
        }

        await repository.DeleteWithRelatedEntitiesAsync(inventoryItem, cancellationToken);

        return Unit.Value;
    }
}
