using HomeHub.Application.Common.Pagination;
using HomeHub.Application.Features.Inventory.Interfaces;
using HomeHub.Domain.Entities;
using HomeHub.Infrastructure.Data;
using HomeHub.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace HomeHub.Infrastructure.Data.Repositories;

public class InventoryItemRepository(HomeHubContext context) : RepositoryBase<InventoryItem>(context), IInventoryItemRepository
{
    public async Task DeleteWithRelatedEntitiesAsync(InventoryItem inventoryItem, CancellationToken cancellationToken)
    {
        // Mark all shopping list items that reference this inventory item as inactive
        var shoppingListItems = await context.ShoppingListItems
            .Where(sli => sli.InventoryItemId == inventoryItem.Id && sli.IsActive)
            .ToListAsync(cancellationToken);

        foreach (var item in shoppingListItems)
        {
            item.MarkAsInactive();
        }

        // Mark all recipe ingredients that reference this inventory item as inactive
        var recipeIngredients = await context.RecipeIngredients
            .Where(ri => ri.InventoryItemId == inventoryItem.Id && ri.IsActive)
            .ToListAsync(cancellationToken);

        foreach (var ingredient in recipeIngredients)
        {
            ingredient.MarkAsInactive();
        }

        // Mark the inventory item as inactive
        inventoryItem.MarkAsInactive();

        // Save all changes
        await context.SaveChangesAsync(cancellationToken);
    }

    public new async Task<InventoryItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _dbSet
            .Where(item => item.Id == id && item.IsActive)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public new async Task<IEnumerable<InventoryItem>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await _dbSet
            .Where(item => item.IsActive)
            .ToListAsync(cancellationToken);
    }

    public new async Task<IEnumerable<InventoryItem>> FindAsync(Expression<Func<InventoryItem, bool>> predicate, CancellationToken cancellationToken)
    {
        return await _dbSet
            .Where(item => item.IsActive)
            .Where(predicate)
            .ToListAsync(cancellationToken);
    }

    public new async Task<PaginationResult<InventoryItem>> GetPaginatedAsync(
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken,
        Expression<Func<InventoryItem, bool>>? predicate = null,
        Func<IQueryable<InventoryItem>, IOrderedQueryable<InventoryItem>>? orderBy = null,
        Func<IQueryable<InventoryItem>, IQueryable<InventoryItem>>? include = null)
    {
        IQueryable<InventoryItem> query = _dbSet.AsNoTracking()
            .Where(item => item.IsActive);

        if (include != null)
            query = include(query);

        if (predicate != null)
            query = query.Where(predicate);

        var totalItems = await query.CountAsync(cancellationToken);

        if (orderBy != null)
            query = orderBy(query);

        query = query.Skip((pageNumber - 1) * pageSize).Take(pageSize);

        var items = await query.ToListAsync(cancellationToken);

        return new PaginationResult<InventoryItem>(items, totalItems, pageNumber, pageSize);
    }
}