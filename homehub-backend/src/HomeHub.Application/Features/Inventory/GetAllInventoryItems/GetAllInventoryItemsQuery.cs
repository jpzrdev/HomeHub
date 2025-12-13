using HomeHub.Application.Common.Pagination;
using HomeHub.Domain.Entities;
using MediatR;

namespace HomeHub.Application.Features.Inventory.GetAll;

public record GetAllInventoryItemsQuery : PaginationQuery, IRequest<PaginationResult<InventoryItem>>
{

}