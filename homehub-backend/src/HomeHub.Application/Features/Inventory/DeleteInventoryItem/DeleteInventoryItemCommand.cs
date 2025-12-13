using MediatR;

namespace HomeHub.Application.Features.Inventory.DeleteInventoryItem;

public record DeleteInventoryItemCommand(Guid Id) : IRequest<Unit>;
