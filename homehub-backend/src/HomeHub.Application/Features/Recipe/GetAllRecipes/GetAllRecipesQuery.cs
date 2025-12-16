using HomeHub.Application.Common.Pagination;
using RecipeEntity = HomeHub.Domain.Entities.Recipe;
using MediatR;

namespace HomeHub.Application.Features.Recipe.GetAllRecipes;

public record GetAllRecipesQuery : PaginationQuery, IRequest<PaginationResult<RecipeEntity>>
{

}

