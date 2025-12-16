using HomeHub.Application.Features.Recipe.Interfaces;
using RecipeEntity = HomeHub.Domain.Entities.Recipe;
using MediatR;

namespace HomeHub.Application.Features.Recipe.GetRecipeById;

public class GetRecipeByIdQueryHandler(IRecipeRepository repository) : IRequestHandler<GetRecipeByIdQuery, RecipeEntity>
{
    public async Task<RecipeEntity> Handle(GetRecipeByIdQuery request, CancellationToken cancellationToken)
    {
        var recipe = await repository.GetByIdWithRelationsAsync(request.Id, cancellationToken);

        if (recipe is null)
        {
            throw new KeyNotFoundException($"Recipe with ID {request.Id} was not found.");
        }

        return recipe;
    }
}