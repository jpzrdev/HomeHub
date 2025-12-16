using HomeHub.Application.Features.Recipe.Interfaces;
using RecipeEntity = HomeHub.Domain.Entities.Recipe;
using HomeHub.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace HomeHub.Infrastructure.Data.Repositories;

public class RecipeRepository(HomeHubContext context) : RepositoryBase<RecipeEntity>(context), IRecipeRepository
{
}

