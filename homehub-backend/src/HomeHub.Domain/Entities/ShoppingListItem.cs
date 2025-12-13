using System.Text.Json.Serialization;
using HomeHub.Domain.Common;

namespace HomeHub.Domain.Entities;

public class ShoppingListItem : BaseEntity
{
    public Guid ShoppingListId { get; internal set; }

    [JsonIgnore]
    public ShoppingList ShoppingList { get; private set; } = null!;

    public Guid InventoryItemId { get; private set; }

    public InventoryItem InventoryItem { get; private set; } = null!;
    public decimal QuantityToBuy { get; private set; }
    public bool IsPurchased { get; private set; }

    private ShoppingListItem()
    {
        // EF Core constructor
    }

    public ShoppingListItem(Guid shoppingListId, Guid inventoryItemId, decimal quantityToBuy)
    {
        if (quantityToBuy <= 0)
            throw new ArgumentException("Quantity to buy must be greater than zero.", nameof(quantityToBuy));

        ShoppingListId = shoppingListId;
        InventoryItemId = inventoryItemId;
        QuantityToBuy = quantityToBuy;
        IsPurchased = false;
    }

    public void MarkAsPurchased()
    {
        IsPurchased = true;
    }

    public void MarkAsNotPurchased()
    {
        IsPurchased = false;
    }

    public void UpdateQuantity(decimal quantityToBuy)
    {
        if (quantityToBuy <= 0)
            throw new ArgumentException("Quantity to buy must be greater than zero.", nameof(quantityToBuy));

        QuantityToBuy = quantityToBuy;
    }
}
