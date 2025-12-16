using RecipeEntity = HomeHub.Domain.Entities.Recipe;
using MediatR;

namespace HomeHub.Application.Features.Recipe.GetRecipeById;

public record GetRecipeByIdQuery(Guid Id) : IRequest<RecipeEntity>
{

}