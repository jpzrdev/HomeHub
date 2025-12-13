using System.Threading.Tasks;
using HomeHub.Application.Features.Inventory.CreateInventory;
using HomeHub.Application.Features.Inventory.DeleteInventoryItem;
using HomeHub.Application.Features.Inventory.GetAll;
using HomeHub.Application.Features.Inventory.GetInventoryItemById;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace HomeHub.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class InventoryController(IMediator mediator) : ControllerBase
{

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById([FromRoute] Guid id)
    {
        var result = await mediator.Send(new GetInventoryItemByIdQuery(id));
        return Ok(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] GetAllInventoryItemsQuery query)
    {
        var result = await mediator.Send(query);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateInventoryItemCommand command)
    {
        var id = await mediator.Send(command);
        return CreatedAtAction(nameof(GetById), new { id }, id);

    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete([FromRoute] Guid id)
    {
        await mediator.Send(new DeleteInventoryItemCommand(id));
        return NoContent();
    }
}
