using HomeHub.Domain.Entities;
using MediatR;

namespace HomeHub.Application.Features.Inventory.GetInventoryItemById;

public record GetInventoryItemByIdQuery(Guid Id) : IRequest<InventoryItem>
{

}