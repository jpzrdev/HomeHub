using HomeHub.Domain.Common;

namespace HomeHub.Domain.Entities;

public class Recipe : BaseEntity
{
    private readonly List<RecipeStep> _steps = new();
    private readonly List<RecipeIngredient> _ingredients = new();

    public string Title { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public IReadOnlyCollection<RecipeStep> Steps => _steps.OrderBy(s => s.Order).ToList().AsReadOnly();
    public IReadOnlyCollection<RecipeIngredient> Ingredients => _ingredients.AsReadOnly();

    private Recipe()
    {
        // EF Core constructor
    }

    public Recipe(string title, string description)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title cannot be null or empty.", nameof(title));

        Title = title;
        Description = description ?? string.Empty;
    }

    public void AddStep(int order, string description)
    {
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Step description cannot be null or empty.", nameof(description));

        if (_steps.Any(s => s.Order == order))
            throw new InvalidOperationException($"Step with order {order} already exists.");

        var step = new RecipeStep(Id, order, description);
        _steps.Add(step);
    }

    public void AddIngredient(Guid inventoryItemId, decimal quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be greater than zero.", nameof(quantity));

        if (_ingredients.Any(i => i.InventoryItemId == inventoryItemId))
            throw new InvalidOperationException("Ingredient with this inventory item already exists in the recipe.");

        var ingredient = new RecipeIngredient(Id, inventoryItemId, quantity);
        _ingredients.Add(ingredient);
    }
}

