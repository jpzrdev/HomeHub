using HomeHub.Application.Common.Pagination;
using HomeHub.Application.Features.Recipe.GetAllRecipes;
using HomeHub.Application.Features.Recipe.Interfaces;
using RecipeEntity = HomeHub.Domain.Entities.Recipe;
using MediatR;

namespace HomeHub.Application.Features.Recipe.GetAllRecipes;

public class GetAllRecipesQueryHandler(IRecipeRepository repository) : IRequestHandler<GetAllRecipesQuery, PaginationResult<RecipeEntity>>
{
    public async Task<PaginationResult<RecipeEntity>> Handle(GetAllRecipesQuery request, CancellationToken cancellationToken)
    {
        var paginatedResult = await repository.GetPaginatedAsync(
            request.PageNumber,
            request.PageSize,
            cancellationToken
        );

        return paginatedResult;
    }
}

