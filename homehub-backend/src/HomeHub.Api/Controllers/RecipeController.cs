using HomeHub.Api.DTOs;
using HomeHub.Application.Features.Recipe.CreateRecipe;
using HomeHub.Application.Features.Recipe.GetAllRecipes;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace HomeHub.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RecipeController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] GetAllRecipesQuery query)
    {
        var result = await mediator.Send(query);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateRecipeRequest request)
    {
        var command = new CreateRecipeCommand(
            request.Title,
            request.Description,
            request.Steps.Select(s => new RecipeStepDto(s.Order, s.Description)).ToList(),
            request.Ingredients.Select(i => new RecipeIngredientDto(i.InventoryItemId, i.Quantity)).ToList()
        );

        var id = await mediator.Send(command);
        return CreatedAtAction(nameof(Create), new { id }, id);
    }
}

