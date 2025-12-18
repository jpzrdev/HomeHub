using System.Text.Json.Serialization;
using HomeHub.Domain.Common;

namespace HomeHub.Domain.Entities;

public class RecipeIngredient : BaseEntity
{
    public Guid RecipeId { get; internal set; }

    [JsonIgnore]
    public Recipe Recipe { get; private set; } = null!;

    public Guid InventoryItemId { get; private set; }

    public InventoryItem InventoryItem { get; private set; } = null!;
    public decimal Quantity { get; private set; }
    public bool IsActive { get; private set; } = true;

    private RecipeIngredient()
    {
        // EF Core constructor
    }

    public RecipeIngredient(Guid recipeId, Guid inventoryItemId, decimal quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be greater than zero.", nameof(quantity));

        RecipeId = recipeId;
        InventoryItemId = inventoryItemId;
        Quantity = quantity;
        IsActive = true;
    }

    public void UpdateQuantity(decimal quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be greater than zero.", nameof(quantity));

        Quantity = quantity;
    }

    public void MarkAsInactive()
    {
        IsActive = false;
    }
}

