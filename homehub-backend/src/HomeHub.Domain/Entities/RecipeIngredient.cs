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
    }

    public void UpdateQuantity(decimal quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be greater than zero.", nameof(quantity));

        Quantity = quantity;
    }
}

