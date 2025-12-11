using HomeHub.Application.Common.Pagination;
using HomeHub.Application.Features.Inventory.GetAll;
using HomeHub.Application.Features.Inventory.Interfaces;
using HomeHub.Domain.Entities;
using MediatR;

namespace HomeHub.Application.Features.Inventory.GetAllInventoryItems;

public class GetAllInventoryItemsQueryHandler(IInventoryItemRepository repository) : IRequestHandler<GetAllInventoryItemsQuery, PaginationResult<InventoryItem>>
{
    public async Task<PaginationResult<InventoryItem>> Handle(GetAllInventoryItemsQuery request, CancellationToken cancellationToken)
    {
        var paginatedResult = await repository.GetPaginatedAsync(request.PageNumber, request.PageSize, cancellationToken);

        return paginatedResult;
    }
}