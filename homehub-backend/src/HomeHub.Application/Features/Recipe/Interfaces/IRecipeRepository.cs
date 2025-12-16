using HomeHub.Application.Interfaces;
using RecipeEntity = HomeHub.Domain.Entities.Recipe;

namespace HomeHub.Application.Features.Recipe.Interfaces;

public interface IRecipeRepository : IRepositoryBase<RecipeEntity>
{ }

