using HomeHub.Application.Features.Recipe.Interfaces;
using RecipeEntity = HomeHub.Domain.Entities.Recipe;
using HomeHub.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace HomeHub.Infrastructure.Data.Repositories;

public class RecipeRepository(HomeHubContext context) : RepositoryBase<RecipeEntity>(context), IRecipeRepository
{
    public async Task<RecipeEntity?> GetByIdWithRelationsAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _context.Set<RecipeEntity>()
            .Include(r => r.Steps)
            .Include(r => r.Ingredients)
                .ThenInclude(i => i.InventoryItem)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }
}

