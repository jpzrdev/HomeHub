using HomeHub.Application.Features.Inventory.CreateInventory;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace HomeHub.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class InventoryController(IMediator mediator) : ControllerBase
{

    [HttpGet("{id}")]
    public IActionResult GetById(Guid id) => Ok($"{id}");

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateInventoryItemCommand command)
    {
        var id = await mediator.Send(command);
        return CreatedAtAction(nameof(GetById), new { id }, id);

    }
}
